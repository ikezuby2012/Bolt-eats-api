using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Notification;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using SharedKernel;

namespace Application.Notification.RegisterDeviceToken;

internal sealed class RegisterDeviceTokenCommandHandler(IApplicationDbContext db, IUserContext userContext) : ICommandHandler<RegisterDeviceTokenCommand>
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "<Pending>")]
    public async Task<Result> Handle(RegisterDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        Domain.Notification.DeviceToken? existing = await db.DeviceTokens.FirstOrDefaultAsync(d => d.Token == request.Token, cancellationToken);

        if (existing is not null)
        {
            existing.UserId = userContext.UserId;
            existing.Platform = request.Platform.ToLowerInvariant();
            existing.IsActive = true;
        }
        else
        {
            db.DeviceTokens.Add(new DeviceToken
            {
                Id = Guid.NewGuid(),
                UserId = userContext.UserId,
                Token = request.Token,
                Platform = request.Platform.ToLowerInvariant(),
                IsActive = true
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
