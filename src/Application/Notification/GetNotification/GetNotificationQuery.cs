using Application.Abstractions.Messaging;
using Application.Notification.Dto;
using SharedKernel;

namespace Application.Notification.GetNotification;

public sealed record GetNotificationQuery(
    bool UnreadOnly = false,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<NotificationDto>>;
