using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Payment.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Payment.GetPaymentStatus;


internal sealed class GetPaymentStatusQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetPaymentStatusQuery, PaymentDto>
{
    public async Task<Result<PaymentDto>> Handle(
        GetPaymentStatusQuery request,
        CancellationToken cancellationToken)
    {
        PaymentDto? payment = await db.Payment
            .AsNoTracking()
            .Where(p => p.Id == request.PaymentId &&
                        p.CustomerId == request.UserId)      // ownership check
            .Select(p => (PaymentDto)p)
            .FirstOrDefaultAsync(cancellationToken);

        if (payment is null)
        {
            return Result.Failure<PaymentDto>(
              Error.NotFound("Payment.NotFound", "Payment not found."));
        }

        return Result.Success(payment);
    }
}
