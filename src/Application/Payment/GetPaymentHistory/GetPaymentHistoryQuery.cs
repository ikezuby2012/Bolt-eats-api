using Application.Abstractions.Messaging;
using Application.Payment.Dto;
using SharedKernel;

namespace Application.Payment.GetPaymentHistory;

public sealed record GetPaymentHistoryQuery(Guid UserId, int Page = 1, int PageSize = 20) : IQuery<PaginatedResult<PaymentHistoryDto>>;
