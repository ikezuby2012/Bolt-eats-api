using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Payments;
using Application.Payment.Dto;
using Domain.Common;
using Domain.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Application.Payment.HandleStripeWebhook;

internal sealed class HandlerStripeWebhookCommandHandler(IApplicationDbContext db, [FromKeyedServices("stripe")] IWebhookParser webhookParser, IDateTimeProvider dateTimeProvider) : ICommandHandler<HandleStripeWebhookCommand>
{
    public async Task<Result> Handle(HandleStripeWebhookCommand command, CancellationToken cancellationToken)
    {
        WebhookParseResult parsed = webhookParser.Parse(command.RawBody, command.StripeSignature);

        if (!parsed.IsValid)
        {
            return Result.Failure(CommonErrors.CustomErrorMessage("Invalid webhook signature."));
        }

        return parsed.EventType switch
        {
            WebhookEventType.PaymentSucceeded =>
                await HandlePaymentSucceededAsync(parsed, cancellationToken),
            WebhookEventType.PaymentFailed =>
                await HandlePaymentFailedAsync(parsed, cancellationToken),
            WebhookEventType.PaymentRefunded =>
                await HandleRefundedAsync(parsed, cancellationToken),
            _ => Result.Success()
        };
    }

    private async Task<Result> HandlePaymentSucceededAsync(
        WebhookParseResult parsed,
        CancellationToken cancellationToken)
    {
        Domain.Payment.Payment? payment = await db.Payment
            .FirstOrDefaultAsync(p => p.GatewayReference == parsed.GatewayReference, cancellationToken);

        if (payment is null)
        {
            return Result.Success();
        }

        if (payment.StatusId == PaymentStatus.Succeeded.Id)
        {
            return Result.Success();
        }

        payment.StatusId = PaymentStatus.Succeeded.Id;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result> HandlePaymentFailedAsync(WebhookParseResult parsed, CancellationToken cancellationToken)
    {
        Domain.Payment.Payment? payment = await db.Payment.FirstOrDefaultAsync(p => p.GatewayReference == parsed.GatewayReference, cancellationToken);

        if (payment is null)
        {
            return Result.Success();
        }

        if (payment.StatusId == PaymentStatus.Failed.Id)
        {
            return Result.Success();
        }

        payment.StatusId = PaymentStatus.Failed.Id;
        payment.FailureCode = parsed.FailureCode;
        payment.FailureMessage = parsed.FailureMessage;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result> HandleRefundedAsync(
        WebhookParseResult parsed,
        CancellationToken cancellationToken)
    {
        Domain.Payment.Payment? payment = await db.Payment
            .FirstOrDefaultAsync(p => p.GatewayReference == parsed.GatewayReference, cancellationToken);

        if (payment is null)
        {
            return Result.Success();
        }

        payment.StatusId = parsed.IsFullRefund
            ? PaymentStatus.Refunded.Id
            : PaymentStatus.PartialRefund.Id;
        payment.RefundReference = parsed.RefundReference;
        payment.RefundAmount = parsed.RefundAmount;
        payment.RefundedAt = dateTimeProvider.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
