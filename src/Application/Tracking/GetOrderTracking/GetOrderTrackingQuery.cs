using Application.Abstractions.Messaging;
using Application.Tracking.Dto;

namespace Application.Tracking.GetOrderTracking;

public sealed record GetOrderTrackingQuery(
    Guid OrderId,
    Guid UserId)
    : IQuery<OrderTrackingDto>;
