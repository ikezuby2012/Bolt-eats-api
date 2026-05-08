using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Payments;
using Application.Payment.Dto;
using Domain.Common;
using Domain.Payment;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Payment.ProcessRefund;

internal sealed class ProcessRefundCommandHandler(IApplicationDbContext db, IDateTimeProvider dateTimeProvider, IPaymentGatewayFactory factory) : ICommandHandler<ProcessRefundCommand, string>
{
    public async Task<Result<string>> Handle(ProcessRefundCommand command, CancellationToken cancellationToken)
    {
        Domain.Payment.Payment? payment = await db.Payment
            .FirstOrDefaultAsync(
                p => p.OrderId == command.OrderId &&
                     p.StatusId == PaymentStatus.Succeeded.Id,
                cancellationToken);

        if (payment is null)
        {
            return Result.Success("No succeeded payment — order may have been placed without payment");
        }

        IPaymentGateway gateway = factory.GetGateway(payment.GatewayId);
        RefundResult refundResult = await gateway.RefundAsync(new RefundRequest(
            GatewayReference: payment.GatewayReference,
            Amount: payment.Amount,
            Reason: command.Reason),
            cancellationToken);

        if (!refundResult.IsSuccess)
        {
            return Result.Failure<string>(CommonErrors.CustomErrorMessage($"Refund failed: {refundResult.FailureMessage}"));
        }

        payment.Status = PaymentStatus.Refunded;
        payment.RefundReference = refundResult.RefundReference;
        payment.RefundAmount = payment.Amount;
        payment.RefundedAt = dateTimeProvider.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success("Operation Successful");
    }
}
