using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Order;
using Domain.PromoCode;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.CancelOrder;

internal sealed class CancelOrderCommandHandler(IApplicationDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<CancelOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        Domain.Order.Order? order = await context.Order
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<Guid>(Domain.Common.CommonErrors.CustomErrorMessage("Order not found"));
        }

        Domain.Users.User? customer = await context.Users
           .Include(u => u.Addresses)
           .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (customer is null)
        {
            return Result.Failure<Guid>(Domain.Common.CommonErrors.CustomErrorMessage("No Customer was found"));
        }

        if (customer.RoleId == UserRole.User.Id && order.CustomerId == userId)
        {
            return Result.Failure<Guid>(Domain.Common.CommonErrors.CustomErrorMessage("Order not found"));
        }

        string requestingRole = Domain.Users.UserRole.FromValue(customer.RoleId)!.Name;

        if (!OrderStateMachine.CanTransition(requestingRole, EOrderStatus.FromValue(order.OrderStatusId)!, EOrderStatus.Cancelled))
        {
            return Result.Failure<Guid>(Domain.Common.CommonErrors.CustomErrorMessage($"Order cannot be cancelled in its current status ({EOrderStatus.FromValue(order.OrderStatusId)!.Name})."));
        }

        order.OrderStatusId = EOrderStatus.Cancelled.Id;
        order.UpdatedAt = dateTimeProvider.UtcNow;
        order.CancelledAt = dateTimeProvider.UtcNow;
        order.CancellationNotes = command.Reason;

        if (order.PromoCode is not null)
        {
            PromoCode? promoCode = await context.PromoCode.FirstOrDefaultAsync(x => x.RestaurantId == order.RestaurantId && x.Code == order.PromoCode, cancellationToken);

            if (promoCode is null)
            {
                return Result.Failure<Guid>(Domain.Common.CommonErrors.CustomErrorMessage("Promo Code was not found"));
            }

            PromoCodeUsage? usage = await context.PromoCodeUsages
               .FirstOrDefaultAsync(
                   u => u.UserId == userId && u.StatusId == PromoUsageStatus.Pending.Id, cancellationToken);

            if (usage is not null)
            {
                usage.Status = PromoUsageStatus.Cancelled;

                await context.PromoCodeUsages
                    .Where(p => p.Id == usage.PromoCodeId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.TimesUsed, p => p.TimesUsed + 1), cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
