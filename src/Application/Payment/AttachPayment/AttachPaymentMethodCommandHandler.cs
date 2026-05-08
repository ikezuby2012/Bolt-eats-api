using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Payments;
using Application.Payment.Dto;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Payment.AttachPayment;

internal sealed class AttachPaymentMethodCommandHandler(IApplicationDbContext db, IUserContext userContext, IPaymentGatewayFactory factory) : ICommandHandler<AttachPaymentMethodCommand, PaymentMethodDto>
{
    public async Task<Result<PaymentMethodDto>> Handle(AttachPaymentMethodCommand command, CancellationToken cancellationToken)
    {
        Domain.Users.User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<PaymentMethodDto>(CommonErrors.CustomErrorMessage("No User was found"));
        }

        IPaymentGateway gateway = factory.GetGateway(command.GatewayId);
        string? customerId = user.GetGatewayCustomerId(gateway.Gateway);

        if (string.IsNullOrEmpty(customerId))
        {
            return Result.Failure<PaymentMethodDto>(CommonErrors.CustomErrorMessage("No gateway customer profile found. Create a payment intent first."));
        }

        AttachMethodResult attachResult = await gateway.AttachPaymentMethodAsync(customerId, command.PaymemtMethodToken, cancellationToken);

        if (attachResult.IsSuccess)
        {
            return Result.Failure<PaymentMethodDto>(CommonErrors.CustomErrorMessage(attachResult.FailureMessage ?? ""));
        }

        IReadOnlyList<SavedPaymentMethod> methods = await gateway.ListPaymentMethodsAsync(customerId, cancellationToken);
        SavedPaymentMethod added = methods.FirstOrDefault(m => m.Id == command.PaymemtMethodToken)
                   ?? methods[0];

        return new PaymentMethodDto(added.Id, added.Brand, added.Last4,
                added.ExpMonth, added.ExpYear, added.IsDefault);
    }
}
