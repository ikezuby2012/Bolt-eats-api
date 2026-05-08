using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Payments;
using Application.Payment.Dto;
using Domain.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Application.Payment.HandleMonnfiyWebhook;

internal sealed class HandleMonnfiyWebhookCommandHandler(IApplicationDbContext db, [FromKeyedServices("monnify")] IWebhookParser webhookParser, IDateTimeProvider dateTimeProvider) : ICommandHandler<HandleMonnifyWebhookCommand>
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

            return Result.Failure(Domain.Common.CommonErrors.CustomErrorMessage("Amount mismatch — payment rejected."));
        }

        payment.StatusId = PaymentStatus.Succeeded.Id;
        await db.SaveChangesAsync(cancellationToken);
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


        if (payment.StatusId == PaymentStatus.Succeeded.Id)
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
        Domain.Payment.Payment? payment = await db.Payment.FirstOrDefaultAsync(
            p => p.GatewayReference == parsed.GatewayReference &&
                 p.GatewayId == PaymentGateway.Monnify.Id,
            cancellationToken);

        if (payment is null)
        {
            return Result.Success();
        }

        payment.StatusId = PaymentStatus.Refunded.Id;
        payment.RefundReference = parsed.RefundReference;
        payment.RefundAmount = parsed.RefundAmount;
        payment.RefundedAt = dateTimeProvider.UtcNow;
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

}
