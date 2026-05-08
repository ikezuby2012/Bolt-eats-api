using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Payments;
using Application.Payment.Dto;
using Domain.Common;
using Domain.Payment;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Payment.ConfirmPayment;

internal sealed class ConfirmPaymentCommandHandler(IApplicationDbContext context, IUserContext userContext, IPaymentGatewayFactory factory, IDateTimeProvider dateTimeProvider) : ICommandHandler<ConfirmPaymentCommand, PaymentConfirmationDto>
{
    public async Task<Result<PaymentConfirmationDto>> Handle(ConfirmPaymentCommand command, CancellationToken cancellationToken)
    {
        Domain.Payment.Payment? payment = await context.Payment
              .FirstOrDefaultAsync(p =>
                  p.Id == command.PaymentId &&
                  p.CustomerId == userContext.UserId &&
                  p.StatusId == PaymentStatus.Pending.Id,
                  cancellationToken);

        if (payment is null)
        {
            return Result.Failure<PaymentConfirmationDto>(
                CommonErrors.CustomErrorMessage("Payment not found or already processed."));
        }


        // 1. Confirm with gateway
        IPaymentGateway gateway = factory.GetGateway(payment.GatewayId);
        ConfirmPaymentResult confirmResult = await gateway.ConfirmPaymentAsync(
            payment.GatewayReference, cancellationToken);

        if (!confirmResult.IsSuccess)
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureCode = confirmResult.FailureCode;
            payment.FailureMessage = confirmResult.FailureMessage;
            await context.SaveChangesAsync(cancellationToken);
            return Result.Failure<PaymentConfirmationDto>(
                CommonErrors.CustomErrorMessage(confirmResult.FailureMessage!));
        }

        payment.StatusId = PaymentStatus.Succeeded.Id;
        payment.CustomerNotes = command.CustomerNotes ?? "";
        payment.CreatedAt = dateTimeProvider.UtcNow;

        payment.Raise(new PaymentConfirmedEvent(payment.Id, command.CustomerNotes));

        await context.SaveChangesAsync(cancellationToken);

        return new PaymentConfirmationDto(
            PaymentId: payment.Id,
            OrderId: payment.OrderId,
            Status: PaymentStatus.Succeeded.Name,
            Amount: payment.Amount,
            Currency: payment.Currency, PaidAt: dateTimeProvider.UtcNow);

    }
}
