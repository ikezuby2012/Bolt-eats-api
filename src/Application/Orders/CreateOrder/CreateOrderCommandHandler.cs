using System.Security.Cryptography;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Abstractions.Services.Rider;
using Application.Orders.Dto;
using Domain.Common;
using Domain.Order;
using Domain.PromoCode;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.CreateOrder;

internal sealed class CreateOrderCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IDeliveryFeeService deliveryFeeService,
    IPromoCodeService promoService,
    IDateTimeProvider dateTimeProvider,
    //IRiderAssignmentService riderAssignmentService,
    IDeliveryEstimateService estimateService)
    : ICommandHandler<CreateOrderCommand, OrderDto>
{
    private const decimal TaxRate = 0.085m;

    public async Task<Result<OrderDto>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;


        Domain.Cart.Cart? cart = await context.Cart
            .Include(c => c.Restaurant)
                .ThenInclude(r => r.Addresses)
            .Include(c => c.Items)
                .ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is null)
        {
            return Result.Failure<OrderDto>(
                CommonErrors.CustomErrorMessage("Cart not found."));
        }


        if (!cart.Items.Any())
        {
            return Result.Failure<OrderDto>(
               CommonErrors.CustomErrorMessage("Cannot place an order with an empty cart."));
        }


        if (!cart.Restaurant.IsActive || !cart.Restaurant.IsOpen)
        {
            return Result.Failure<OrderDto>(
                CommonErrors.CustomErrorMessage("Restaurant is currently closed."));
        }

        Order? existingOrder = await context.Order
          .AsNoTracking()
          .Include(o => o.Items)
          .Include(o => o.Restaurant)
          .FirstOrDefaultAsync(
              o => o.CartId == cart.Id && o.CustomerId == userId,
              cancellationToken);

        if (existingOrder is not null)
        {
            return (OrderDto)existingOrder;
        }


        var unavailable = cart.Items
            .Where(i => !i.MenuItem.IsAvailable)
            .Select(i => i.MenuItem.Name)
            .ToList();

        if (unavailable.Count != 0)
        {
            return Result.Failure<OrderDto>(
                CommonErrors.CustomErrorMessage(
                    $"The following items are no longer available: {string.Join(", ", unavailable)}."));
        }


        // ── 2. Load customer + addresses ──────────────────────────────────
        Domain.Users.User? customer = await context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (customer is null)
        {
            return Result.Failure<OrderDto>(
               CommonErrors.CustomErrorMessage("Customer not found."));
        }


        if (customer.Addresses is null || !customer.Addresses.Any())
        {
            return Result.Failure<OrderDto>(
               CommonErrors.CustomErrorMessage(
                   "No delivery address found. Please add one before ordering."));
        }


        Domain.Address.Address? deliveryAddress = command.AddressId.HasValue
            ? customer.Addresses.FirstOrDefault(a => a.Id == command.AddressId.Value)
            : customer.Addresses.FirstOrDefault();

        if (deliveryAddress is null)
        {
            return Result.Failure<OrderDto>(
                CommonErrors.CustomErrorMessage(
                    "The selected delivery address was not found on your account."));
        }


        string contactEmail = string.IsNullOrWhiteSpace(command.ContactEmail)
            ? customer.Email
            : command.ContactEmail;

        string? contactPhone = string.IsNullOrWhiteSpace(command.ContactPhone)
            ? customer.PhoneNumber
            : command.ContactPhone;

        string? contactName = string.IsNullOrWhiteSpace(command.ContactName)
            ? $"{customer.FirstName} {customer.LastName}"
            : command.ContactName;

        // ── 4. Calculate totals ───────────────────────────────────────────
        decimal subtotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);

        if (cart.Restaurant.MinOrderAmount.HasValue &&
            subtotal < cart.Restaurant.MinOrderAmount)
        {
            return Result.Failure<OrderDto>(
               CommonErrors.CustomErrorMessage(
                   $"Minimum order for this restaurant is ₦{cart.Restaurant.MinOrderAmount:N0}."));
        }


        decimal deliveryFee = await deliveryFeeService.CalculateAsync(
            cart.Restaurant, deliveryAddress, cancellationToken);

        decimal discount = 0m;
        if (cart.PromoCode is not null && cart.PromoDiscount.HasValue)
        {
            PromoValidationResult promoCheck = await promoService.ValidatePromoCodeAsync(
                cart.PromoCode,
                userId,
                cart.RestaurantId,
                subtotal,
                cancellationToken);

            discount = CalculateDiscount(
                subtotal,
                promoCheck,
                cart.PromoDiscountType?.ToUpperInvariant() == "PERCENTAGE",
                cart.PromoDiscount);
        }

        decimal taxableAmount = subtotal - discount;
        decimal tax = Math.Round(taxableAmount * TaxRate, 2);
        decimal total = taxableAmount + deliveryFee + tax;

        // ── 5. Delivery estimate ──────────────────────────────────────────
        Tracking.Dto.DeliveryEstimate estimate = await estimateService.EstimateAsync(
            cart.Restaurant, deliveryAddress, cancellationToken);

        var orderId = Guid.NewGuid();

        int random = RandomNumberGenerator.GetInt32(100, 1000);
        // ── 6. Build order ────────────────────────────────────────────────
        var order = new Order
        {
            Id = orderId,
            CustomerId = userId,
            RestaurantId = cart.RestaurantId,
            AddressId = deliveryAddress.Id,
            CartId = cart.Id,
            ContactEmail = contactEmail,
            ContactPhone = contactPhone,
            ContactName = contactName,

            OrderStatusId = EOrderStatus.AwaitingPayment.Id,
            DeliveryFee = deliveryFee,
            Discount = discount,
            Tax = tax,
            Total = total,
            SubTotal = subtotal,
            PromoCode = cart.PromoCode,
            Notes = command.CustomerNotes,
            EstimatedDeliveryMinutes = estimate.TotalMinutes,
            EstimatedTravelMinutes = estimate.TravelMinutes,
            CreatedAt = dateTimeProvider.UtcNow,
            CreatedBy = userId.ToString(),
            OrderCode = $"ORD{DateTime.UtcNow:ddHHmmss}{random}",
            Items = cart.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                MenuItemId = i.MenuItemId,
                Name = i.MenuItem.Name,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                CreatedAt = dateTimeProvider.UtcNow,
                CreatedBy = userId.ToString(),
            }).ToList()
        };

        context.Order.Add(order);

        // ── 7. Redeem promo ───────────────────────────────────────────────
        if (cart.PromoCode is not null)
        {
            PromoCode? promoCode = await context.PromoCode
                .FirstOrDefaultAsync(
                    x => x.RestaurantId == cart.RestaurantId &&
                         x.Code == cart.PromoCode,
                    cancellationToken);

            if (promoCode is null)
            {
                return Result.Failure<OrderDto>(
                   CommonErrors.CustomErrorMessage("Promo code was not found."));
            }


            PromoCodeUsage? usage = await context.PromoCodeUsages
                .FirstOrDefaultAsync(
                    u => u.UserId == userId &&
                         u.StatusId == PromoUsageStatus.Pending.Id,
                    cancellationToken);

            if (usage is not null)
            {
                usage.PromoCodeId = promoCode.Id;
                usage.StatusId = PromoUsageStatus.Redeemed.Id;
                usage.RedeemedAt = dateTimeProvider.UtcNow;
                usage.UpdatedBy = userId.ToString();
                usage.DiscountApplied = discount;
                usage.UpdatedAt = dateTimeProvider.UtcNow;

                await context.PromoCodeUsages
                    .Where(p => p.Id == usage.Id)           // fix: was usage.PromoCodeId
                    .ExecuteUpdateAsync(
                        s => s.SetProperty(
                            p => p.TimesUsed, p => p.TimesUsed + 1),
                        cancellationToken);
            }
        }

        // ── 8. Invalidate cart ────────────────────────────────────────────
        cart.PromoCode = null;
        cart.PromoDiscount = null;
        cart.PromoDiscountType = null;

        await context.SaveChangesAsync(cancellationToken);

        // ── 9. Auto-assign rider (fire and forget) ────────────────────────
        //_ = riderAssignmentService.TryAutoAssignAsync(order.Id, cancellationToken);

        return (OrderDto)order;
    }

    private static decimal CalculateDiscount(
        decimal subtotal,
        PromoValidationResult promoCheck,
        bool isPercentage,
        decimal? promoDiscount = 0)
    {
        if (!promoCheck.IsValid)
        {
            return 0m;
        }


        return isPercentage
            ? Math.Round(subtotal * ((promoDiscount ?? 0m) / 100m), 2)
            : (promoDiscount ?? 0m);
    }
}
