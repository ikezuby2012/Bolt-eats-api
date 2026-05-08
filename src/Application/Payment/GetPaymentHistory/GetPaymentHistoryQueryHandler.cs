using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Payment.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Payment.GetPaymentHistory;

internal sealed class GetPaymentHistoryQueryHandler(IApplicationDbContext context) : IQueryHandler<GetPaymentHistoryQuery, PaginatedResult<PaymentHistoryDto>>
{
    public async Task<Result<PaginatedResult<PaymentHistoryDto>>> Handle(GetPaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        IOrderedQueryable<Domain.Payment.Payment> baseQuery = context.Payment.AsNoTracking().Where(p => p.CustomerId == request.UserId).OrderByDescending(p => p.CreatedAt);

        int total = await baseQuery.CountAsync(cancellationToken);

        List<PaymentHistoryDto> items = await baseQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => (PaymentHistoryDto)p)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<PaymentHistoryDto>
        {
            Data = items,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalItems = total,
        };
    }
}
