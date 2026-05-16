using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Order;
using Domain.Rider;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tracking.AdvanceOrderStatus;

internal class AdvanceOrderStatusCommandHandler(IApplicationDbContext db) : ICommandHandler<AdvanceOrderStatusCommand>
{
    public async Task<Result> Handle(AdvanceOrderStatusCommand command, CancellationToken cancellationToken)
    {
        Order? order = await db.Order.FirstOrDefaultAsync(
            o => o.Id == command.OrderId &&
                 o.RiderId == command.RiderId,
            cancellationToken);

        if (order is null)
        {
            return Result.Failure(Domain.Common.CommonErrors.CustomErrorMessage("No active order found for this rider!"));
        }

        //string oldStatus = EOrderStatus.FromValue(order.OrderStatusId)!.Name;
        EOrderStatus newStatus = EOrderStatus.FromNameOrDefault(command.NewStatus)!;

        order.OrderStatusId = newStatus.Id;

      //  order.Raise(new BroadcastStatusChangeDomain(order.Id, new OrderStatusChanged(order.Id, oldStatus, newStatus.Name, ChangedAt: DateTime.UtcNow)));

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
