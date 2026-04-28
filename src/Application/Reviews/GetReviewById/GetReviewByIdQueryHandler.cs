using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Reviews.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reviews.GetReviewById;

internal sealed class GetReviewByIdQueryHandler(IApplicationDbContext context) : IQueryHandler<GetReviewByIdQuery, ReviewDto>
{
    public async Task<Result<ReviewDto>> Handle(GetReviewByIdQuery query, CancellationToken cancellationToken)
    {
        Domain.Review.Review? review = await context.Review
            .AsNoTracking()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken);

        return review is null
            ? Result.Failure<ReviewDto>(Domain.Common.CommonErrors.CustomErrorMessage("Review not found."))
            : (ReviewDto)review;
    }
}
