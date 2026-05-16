using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using SharedKernel;

namespace Application.Notification.UnregisterDeviceToken;

internal sealed class UnregisterDeviceTokenCommandHandler(IApplicationDbContext db, IUserContext userContext) : ICommandHandler<UnregisterDeviceTokenCommand>
{
    public async Task<Result> Handle(UnregisterDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        await db.DeviceTokens
             .Where(d => d.Token == request.Token &&
                         d.UserId == userContext.UserId &&
                         d.IsActive)
             .ExecuteUpdateAsync(
                 s => s.SetProperty(d => d.IsActive, false),
                 cancellationToken);

        return Result.Success();
    }
}
