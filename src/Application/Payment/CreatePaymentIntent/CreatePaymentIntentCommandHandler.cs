using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Abstractions.Services.Payments;
using Application.Payment.Dto;
using Domain.Common;
using Domain.Order;
using Domain.Payment;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Payment.CreatePaymentIntent;

internal class CreatePaymentIntentCommandHandler(IApplicationDbContext context, IUserContext userContext, IPaymentGatewayFactory factory, ICartService cartService, IDateTimeProvider dateTimeProvider) : ICommandHandler<CreatePaymentIntentCommand, PaymentIntentDto>
{
    public async Task<Result<PaymentIntentDto>> Handle(CreatePaymentIntentCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Domain.Order.Order? order = await context.Order.FirstOrDefaultAsync(x => x.Id == command.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<PaymentIntentDto>(CommonErrors.CustomErrorMessage("Order was not found"));
        }


        Domain.Cart.Cart? cart = await context.Cart.Include(c => c.Restaurant).Include(c => c.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(
                c => c.Id == order.CartId &&
                     c.UserId == userId, cancellationToken);

        if (cart is null)
        {
            return Result.Failure<PaymentIntentDto>(CommonErrors.CustomErrorMessage("No Active Cart was found"));
        }

        if (!cart.Items.Any())
        {
            return Result.Failure<PaymentIntentDto>(CommonErrors.CustomErrorMessage("Cart is empty"));
        }


        Domain.Cart.CartSummaryDto summary = cartService.Calculate(cart);

        if (!summary.MeetMinimumOrder)
        {
            return Result.Failure<PaymentIntentDto>(CommonErrors.CustomErrorMessage($"Minimum order amount of {cart.Restaurant.MinOrderAmount:C} not met."));
        }

        Domain.Users.User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<PaymentIntentDto>(CommonErrors.CustomErrorMessage("No User was found"));
        }

        IPaymentGateway gateway = factory.GetGateway(command.GatewayId);

        string gatewayCustomerId = await EnsureGatewayCustomerAsync(
            user!, gateway.Gateway, cancellationToken);

        var paymentId = Guid.NewGuid();

        CreateIntentResult intentResult = await gateway.CreateIntentAsync(new CreateIntentRequest(
            OrderId: paymentId,
            Amount: summary.Total,
            Currency: "NGN",
            GatewayCustomerId: gatewayCustomerId,
            Description: $"Order from {cart.Restaurant.Name}",
            Metadata: new Dictionary<string, string>
            {
                ["cart_id"] = cart.Id.ToString(),
                ["user_id"] = userId.ToString(),
                ["customer_name"] = order.ContactName ?? "customer",
                ["customer_email"] = order.ContactEmail ?? "customer@example.com"
            }),
            cancellationToken);

        if (!intentResult.IsSuccess)
        {
            order.OrderStatusId = EOrderStatus.Cancelled.Id;
            order.CancellationNotes = "Payment initialization failed.";
            await context.SaveChangesAsync(cancellationToken);

            return Result.Failure<PaymentIntentDto>(CommonErrors.CustomErrorMessage(intentResult.FailureMessage!));
        }

        var payment = new Domain.Payment.Payment
        {
            Id = paymentId,
            CustomerId = userId,
            OrderId = command.OrderId,
            GatewayId = gateway.Gateway,
            StatusId = PaymentStatus.Pending.Id,
            Amount = summary.Total,
            Currency = "NGN",
            GatewayReference = intentResult.GatewayReference!,
            ClientSecret = intentResult.ClientSecret,
            GatewayCustomerId = gatewayCustomerId,
            FailureMessage = intentResult.FailureMessage,
            CreatedAt = dateTimeProvider.UtcNow,
            CreatedBy = userId.ToString(),
            IsSoftDeleted = false,
        };

        await context.Payment.AddAsync(payment, cancellationToken);

        //cart.IsSoftDeleted = true;
        await context.SaveChangesAsync(cancellationToken);

        return new PaymentIntentDto(PaymentId: payment.Id,
            GatewayReference: payment.GatewayReference,
            ClientSecret: payment.ClientSecret!,
            Amount: payment.Amount,
            Currency: payment.Currency,
            Gateway: payment.Gateway,
            CheckoutLink: gateway.Gateway == Domain.Payment.PaymentGateway.Monnify.Id ? intentResult.ClientSecret : null);
    }

    private async Task<string> EnsureGatewayCustomerAsync(
        User user,
        int gatewayId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(user.StripeCustomerId) && gatewayId == Domain.Payment.PaymentGateway.Stripe.Id)
        {
            return user.StripeCustomerId;
        }

        if (!string.IsNullOrEmpty(user.MonnifyCustomerId) && gatewayId == Domain.Payment.PaymentGateway.Monnify.Id)
        {
            return user.MonnifyCustomerId;
        }

        IPaymentGateway gateway = factory.GetGateway(gatewayId);

        string customerId = await gateway.CreateCustomerAsync(user, cancellationToken);

        if (gateway.Gateway == Domain.Payment.PaymentGateway.Stripe.Id)
        {
            user.StripeCustomerId = customerId;
        }
        else
        {
            user.MonnifyCustomerId = customerId;
        }

        return customerId;
    }
}
