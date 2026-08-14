namespace Application.Abstractions.Services.Order;

public interface IOrderHubService
{
    Task NotifyOrderStatusChangedAsync(
        Guid orderId,
        Guid customerId,
        Guid restaurantId,
        Guid? riderId,
        string newStatus,
        string statusLabel,
        DateTime updatedAt,
        CancellationToken cancellationToken = default);

    Task NotifyNewOrderAsync(
        Guid restaurantId,
        object orderSummary,
        CancellationToken cancellationToken = default);

    Task NotifyRiderAssignedAsync(
        Guid riderId,
        object orderSummary,
        CancellationToken cancellationToken = default);
}
