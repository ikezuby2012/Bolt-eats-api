using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Notification;
using Application.Abstractions.Services.Payments;
using Application.Payment.Dto;
using Domain.Notification;
using Domain.Order;
using Domain.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Application.Payment.HandleMonnfiyWebhook;

internal sealed class HandleMonnfiyWebhookCommandHandler(IApplicationDbContext db, [FromKeyedServices("monnify")] IWebhookParser webhookParser, IDateTimeProvider dateTimeProvider,
    IPaymentHubService paymentHubService,
    INotificationService notificationService) : ICommandHandler<HandleMonnifyWebhookCommand>
{
    public async Task<Result> Handle(HandleMonnifyWebhookCommand command, CancellationToken cancellationToken)
    {
        Dto.WebhookParseResult parsed = webhookParser.Parse(command.RawBody, command.MonnifySignature);

        if (!parsed.IsValid)
        {
            return Result.Failure(Domain.Common.CommonErrors.CustomErrorMessage("Invalid Monnify webhook signature or payload."));
        }

        return parsed.EventType switch
        {
            WebhookEventType.PaymentSucceeded =>
                await HandlePaymentSucceededAsync(parsed, cancellationToken),
            WebhookEventType.PaymentFailed =>
                await HandlePaymentFailedAsync(parsed, cancellationToken),
            WebhookEventType.PaymentRefunded =>
                await HandleRefundedAsync(parsed, cancellationToken),
            WebhookEventType.PaymentDisputed =>
                await HandleDisputedAsync(parsed, cancellationToken),

            _ => Result.Success()
        };
        throw new NotImplementedException();
    }

    private async Task<Result> HandlePaymentSucceededAsync(
        WebhookParseResult parsed,
        CancellationToken cancellationToken)
    {
        Domain.Payment.Payment? payment = await db.Payment.FirstOrDefaultAsync(
            p => p.GatewayReference == parsed.GatewayReference &&
                 p.GatewayId == PaymentGateway.Monnify.Id,
            cancellationToken);

        if (payment is null)
        {
            return Result.Success();
        }

        Domain.Order.Order? order = await db.Order.FirstOrDefaultAsync(p => p.Id == payment.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Success();
        }

        Domain.Cart.Cart? cart = await db.Cart.FirstOrDefaultAsync(x => x.Id == order.CartId, cancellationToken);

        if (cart is null)
        {
            return Result.Success();
        }

        if (payment.StatusId == PaymentStatus.Succeeded.Id)
        {
            return Result.Success();
        }

        // Amount tamper check — Monnify can report partial payment
        if (parsed.ReportedAmount < payment.Amount)
        {
            payment.StatusId = PaymentStatus.Failed.Id;
            payment.FailureCode = "amount_mismatch";
            payment.FailureMessage =
                $"Expected {payment.Amount}, gateway reported {parsed.ReportedAmount}.";

            await db.SaveChangesAsync(cancellationToken);

            await NotifyFailureAsync(payment, parsed.FailureMessage ?? "Payment could not be completed.", cancellationToken);

            return Result.Failure(Domain.Common.CommonErrors.CustomErrorMessage("Amount mismatch — payment rejected."));
        }

        payment.StatusId = PaymentStatus.Succeeded.Id;
        order.OrderStatusId = EOrderStatus.Pending.Id;
        cart.IsSoftDeleted = true;
        await db.SaveChangesAsync(cancellationToken);

        await paymentHubService.NotifyPaymentSucceededAsync(payment.CustomerId, payment.Id, order.Id, payment.Amount, cancellationToken);

        // ── 2. FCM push notification ──────────────────────────────────────────
        await notificationService.NotifyAsync(
            userId: payment.CustomerId,
            NotificationTypeId: NotificationType.PaymentSucceeded.Id,
            NotificationChannelId: NotificationChannel.Both.Id,
            title: "Payment Confirmed",
            body: $"Your payment of ₦{payment.Amount:N0} was successful. Order confirmed!",
            payload: new { screen = "OrderDetail", orderId = order.Id },
            cancellationToken: cancellationToken);

        return Result.Success();
    }


    private async Task<Result> HandlePaymentFailedAsync(
        WebhookParseResult parsed,
        CancellationToken cancellationToken)
    {
        Domain.Payment.Payment? payment = await db.Payment.FirstOrDefaultAsync(
            p => p.GatewayReference == parsed.GatewayReference &&
                 p.GatewayId == PaymentGateway.Monnify.Id,
            cancellationToken);

        if (payment is null)
        {
            return Result.Success();
        }

        Order? order = await db.Order.FirstOrDefaultAsync(p => p.Id == payment.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Success();
        }


        if (payment.StatusId == PaymentStatus.Succeeded.Id)
        {
            return Result.Success();
        }

        payment.StatusId = PaymentStatus.Failed.Id;
        payment.FailureCode = parsed.FailureCode;
        payment.FailureMessage = parsed.FailureMessage;
        order.OrderStatusId = EOrderStatus.PaymentFailed.Id;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result> HandleRefundedAsync(
        WebhookParseResult parsed,
        CancellationToken cancellationToken)
    {
        Domain.Payment.Payment? payment = await db.Payment.FirstOrDefaultAsync(
            p => p.GatewayReference == parsed.GatewayReference &&
                 p.GatewayId == PaymentGateway.Monnify.Id,
            cancellationToken);

        if (payment is null)
        {
            return Result.Success();
        }

        Order? order = await db.Order.FirstOrDefaultAsync(p => p.Id == payment.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Success();
        }


        payment.StatusId = PaymentStatus.Refunded.Id;
        payment.RefundReference = parsed.RefundReference;
        payment.RefundAmount = parsed.RefundAmount;
        payment.RefundedAt = dateTimeProvider.UtcNow;
        order.OrderStatusId = EOrderStatus.Refunded.Id;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result> HandleDisputedAsync(
        WebhookParseResult parsed,
        CancellationToken cancellationToken)
    {
        Domain.Payment.Payment? payment = await db.Payment.FirstOrDefaultAsync(
            p => p.GatewayReference == parsed.GatewayReference &&
                  p.GatewayId == PaymentGateway.Monnify.Id,
             cancellationToken);

        if (payment is null)
        {
            return Result.Success();
        }

        payment.StatusId = PaymentStatus.Disputed.Id;   // queryable by Finance
        payment.FailureCode = parsed.FailureCode;
        payment.FailureMessage = parsed.FailureMessage;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task NotifyFailureAsync(Domain.Payment.Payment payment, string reason, CancellationToken cancellationToken)
    {
        await paymentHubService.NotifyPaymentFailedAsync(
            payment.CustomerId,
            payment.Id,
            reason,
            cancellationToken);

        // FCM
        await notificationService.NotifyAsync(
            userId: payment.CustomerId,
            NotificationTypeId: NotificationType.PaymentFailed.Id,
            NotificationChannelId: NotificationChannel.Both.Id,
            title: "Payment Failed",
            body: reason,
            payload: new { screen = "Cart" },
            cancellationToken: cancellationToken);
    }

}
