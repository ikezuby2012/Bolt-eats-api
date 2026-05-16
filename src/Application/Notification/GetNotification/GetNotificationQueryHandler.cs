using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Notification.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Notification.GetNotification;

internal sealed class GetNotificationQueryHandler(IApplicationDbContext db, IUserContext userContext) : IQueryHandler<GetNotificationQuery, PaginatedResult<NotificationDto>>
{
    public async Task<Result<PaginatedResult<NotificationDto>>> Handle(GetNotificationQuery request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        IQueryable<Domain.Notification.Notification> query = db.Notification
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (request.UnreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        int total = await query.CountAsync(cancellationToken);

        List<NotificationDto> items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => (NotificationDto)n)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<NotificationDto>
        {
            Data = items,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalItems = total
        };
    }
}
