using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Payments;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Payment.DetachPaymentMethod;

internal sealed class DetachPaymentMothodHandler(IApplicationDbContext db, IUserContext userContext, IPaymentGatewayFactory factory) : ICommandHandler<DetachPaymentMethodCommand>
{
    public async Task<Result> Handle(DetachPaymentMethodCommand command, CancellationToken cancellationToken)
    {
        Domain.Users.User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (user == null)
        {
            return Result.Failure(CommonErrors.CustomErrorMessage("No User was found"));
        }

        IPaymentGateway gateway = factory.GetGateway(command.GatewayId);
        await gateway.DetachPaymentMethodAsync(command.PaymentMethodId, cancellationToken);

        return Result.Success();
    }
}
