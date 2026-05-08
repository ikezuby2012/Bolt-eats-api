using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Abstractions.Services.Payments;
using Application.Payment.Dto;
using Domain.Common;
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
        Domain.Cart.Cart? cart = await context.Cart.Include(c => c.Restaurant).Include(c => c.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(
                c => c.Id == command.CartId &&
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

        CreateIntentResult intentResult = await gateway.CreateIntentAsync(new CreateIntentRequest(
            OrderId: Guid.Empty,   // order not created yet
            Amount: summary.Total,
            Currency: "NGN",
            GatewayCustomerId: gatewayCustomerId,
            Description: $"Order from {cart.Restaurant.Name}",
            Metadata: new Dictionary<string, string>
            {
                ["cart_id"] = cart.Id.ToString(),
                ["user_id"] = userId.ToString()
            }),
            cancellationToken);

        if (!intentResult.IsSuccess)
        {
            return Result.Failure<PaymentIntentDto>(CommonErrors.CustomErrorMessage(intentResult.FailureMessage!));
        }

        var payment = new Domain.Payment.Payment
        {
            Id = Guid.NewGuid(),
            CustomerId = userId,
            OrderId = Guid.Empty,           // populated after order placement
            GatewayId = gateway.Gateway,
            Status = PaymentStatus.Pending,
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
        await context.SaveChangesAsync(cancellationToken);

        return new PaymentIntentDto(PaymentId: payment.Id,
            GatewayReference: payment.GatewayReference,
            ClientSecret: payment.ClientSecret!,
            Amount: payment.Amount,
            Currency: payment.Currency,
            Gateway: payment.Gateway);
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
