using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Domain.Order;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

internal sealed class RiderAssignmentService(IApplicationDbContext context) : IRiderAssignmentService
{
    public async Task TryAutoAssignAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        Order? order = await context.Order
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            return;
        }

        Domain.Users.User? availableRider = await context.Users
            .Where(u =>
                u.RoleId == Domain.Users.UserRole.Rider.Id &&
                u.IsOnline &&
                !context.Order.Any(o =>
                    o.RiderId == u.Id &&
                    (o.OrderStatusId == EOrderStatus.Cancelled.Id ||
                     o.OrderStatusId == EOrderStatus.ReadyForPickup.Id)))
            //.OrderBy(u =>
            //    u.Addresses.First().L.Distance(order.DeliveryAddress.Location))
            .FirstOrDefaultAsync(cancellationToken);

        if (availableRider is null)
        {
            return;
        }

        order.RiderId = availableRider.Id;
        await context.SaveChangesAsync(cancellationToken);
    }
}
