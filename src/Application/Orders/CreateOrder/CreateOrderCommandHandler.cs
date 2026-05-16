using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Abstractions.Services.Rider;
using Application.Orders.Dto;
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
    IDateTimeProvider dateTimeProvider, IRiderAssignmentService riderAssignmentService,
    IDeliveryEstimateService estimateService) : ICommandHandler<CreateOrderCommand, OrderDto>

{
    private const decimal TaxRate = 0.085m;
    public async Task<Result<OrderDto>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Domain.Cart.Cart? cart = await context.Cart
            .Include(c => c.Restaurant)
                .ThenInclude(x => x.Addresses)
            .Include(c => c.Items)
                .ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(
                c => c.UserId == userId,
                cancellationToken);


        if (cart is null)
        {
            return Result.Failure<OrderDto>(Domain.Common.CommonErrors.CustomErrorMessage("Cart not found"));
        }
        if (!cart.Items.Any())
        {
            return Result.Failure<OrderDto>(Domain.Common.CommonErrors.CustomErrorMessage("Cannot place an order with an empty cart"));
        }

        if (!cart.Restaurant.IsActive || !cart.Restaurant.IsOpen)
        {
            return Result.Failure<OrderDto>(Domain.Common.CommonErrors.CustomErrorMessage("Restaurant is currently closed."));
        }
        var unavailable = cart.Items
            .Where(i => !i.MenuItem.IsAvailable)
            .Select(i => i.MenuItem.Name)
            .ToList();

        if (unavailable.Count != 0)
        {
            return Result.Failure<OrderDto>(Domain.Common.CommonErrors.CustomErrorMessage($"The following items are no longer available: {string.Join(", ", unavailable)}."));
        }

        Domain.Users.User? customer = await context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (customer is null)
        {
            return Result.Failure<OrderDto>(Domain.Common.CommonErrors.CustomErrorMessage("No Customer was found"));
        }

        Domain.Address.Address? customerAddress = customer.Addresses?.FirstOrDefault();
        if (customerAddress is null)
        {
            return Result.Failure<OrderDto>(Domain.Common.CommonErrors.CustomErrorMessage("No delivery address found. Please add one before ordering."));
        }

        decimal subtotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);

        decimal deliveryFee = await deliveryFeeService.CalculateAsync(
            cart.Restaurant,
            customerAddress,
            cancellationToken);

        decimal discount = 0m;
        if (cart.PromoCode is not null && cart.PromoDiscount.HasValue)
        {
            PromoValidationResult promoCheck = await promoService.ValidatePromoCodeAsync(
                cart.PromoCode,
                userId,
                cart.RestaurantId,
                subtotal,
                cancellationToken);

            discount = CalculateDiscount(subtotal, promoCheck, cart.PromoDiscountType?.ToUpperInvariant() == "PERCENTAGE", cart.PromoDiscount);
        }

        decimal taxableAmount = subtotal - discount;
        decimal tax = Math.Round(taxableAmount * TaxRate, 2);
        decimal total = taxableAmount + deliveryFee + tax;

        if (cart.Restaurant.MinOrderAmount.HasValue && subtotal < cart.Restaurant.MinOrderAmount)
        {
            return Result.Failure<OrderDto>(Domain.Common.CommonErrors.CustomErrorMessage($"Minimum order amount for this restaurant is {cart.Restaurant.MinOrderAmount:C}."));
        }

        Tracking.Dto.DeliveryEstimate estimate = await estimateService.EstimateAsync(cart.Restaurant, customerAddress, cancellationToken);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = userId,
            RestaurantId = cart.RestaurantId,
            AddressId = customerAddress.Id,
            OrderStatusId = EOrderStatus.Pending.Id,
            DeliveryFee = deliveryFee,
            Discount = discount,
            Tax = tax,
            Total = total,
            PromoCode = cart.PromoCode,
            Notes = command.CustomerNotes,
            SubTotal = subtotal,
            CreatedAt = dateTimeProvider.UtcNow,
            CreatedBy = userId.ToString(),
            EstimatedDeliveryMinutes = estimate.TotalMinutes,
            EstimatedTravelMinutes = estimate.TravelMinutes,
            Items = cart.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                MenuItemId = i.MenuItemId,
                Name = i.MenuItem.Name,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                CreatedBy = userId.ToString(),
                CreatedAt = dateTimeProvider.UtcNow,
            }).ToList()
        };

        context.Order.Add(order);

        if (cart.PromoCode is not null)
        {
            PromoCode? promoCode = await context.PromoCode.FirstOrDefaultAsync(x => x.RestaurantId == cart.RestaurantId && x.Code == cart.PromoCode, cancellationToken);

            if (promoCode is null)
            {
                return Result.Failure<OrderDto>(Domain.Common.CommonErrors.CustomErrorMessage("Promo Code was not found"));
            }

            PromoCodeUsage? usage = await context.PromoCodeUsages
               .FirstOrDefaultAsync(
                   u => u.UserId == userId && u.StatusId == PromoUsageStatus.Pending.Id, cancellationToken);

            if (usage is not null)
            {
                usage.PromoCodeId = promoCode.Id;
                usage.StatusId = PromoUsageStatus.Redeemed.Id;
                usage.RedeemedAt = dateTimeProvider.UtcNow;
                usage.UpdatedBy = userId.ToString();
                usage.DiscountApplied = discount;
                usage.UpdatedAt = dateTimeProvider.UtcNow;
                usage.RedeemedAt = dateTimeProvider.UtcNow;

                await context.PromoCodeUsages
                    .Where(p => p.Id == usage.PromoCodeId)
                    .ExecuteUpdateAsync(
                        s => s.SetProperty(p => p.TimesUsed, p => p.TimesUsed + 1),
                        cancellationToken);
            }
        }

        cart.PromoCode = null;
        cart.PromoDiscount = null;
        cart.PromoDiscountType = null;
        cart.IsSoftDeleted = true;

        await context.SaveChangesAsync(cancellationToken);

        _ = riderAssignmentService.TryAutoAssignAsync(order.Id, cancellationToken);

        return (OrderDto)order;
    }

    private decimal CalculateDiscount(decimal subtotal, PromoValidationResult promoCheck, bool isPercentage, decimal? promoDiscount = 0)
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

