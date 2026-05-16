using Application.Abstractions.Messaging;
using Application.Tracking.Dto;

namespace Application.Tracking.GetOrderTrackingSnapshot;

public sealed record GetOrderTrackingSnapshotQuery(Guid OrderId) : IQuery<OrderTrackingSnapshotDto>;
