using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Reviews.Dto;
using Domain.Order;
using Domain.Review;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reviews.RemoveReview;

internal class RemoveReviewCommandHandler(IApplicationDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<RemoveReviewCommand>
{
    public async Task<Result> Handle(RemoveReviewCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        Domain.Review.Review? review = await context.Review
            //.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == command.ReviewId, cancellationToken);

        if (review is null)
        {
            return Result.Failure(Domain.Common.CommonErrors.CustomErrorMessage("Review not found!"));
        }
        review.IsSoftDeleted = true;
        review.UpdatedBy = userId.ToString();
        review.UpdatedAt = dateTimeProvider.UtcNow;

        review.Raise(new ReviewRatingUpdateEvent(review.RestaurantId));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
