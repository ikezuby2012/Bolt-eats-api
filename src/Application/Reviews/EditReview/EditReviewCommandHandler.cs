using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Reviews.Dto;
using Domain.Review;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reviews.EditReview;

internal sealed class EditReviewCommandHandler(IApplicationDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<EditReviewCommand, ReviewDto>
{
    private static readonly TimeSpan EditWindow = TimeSpan.FromHours(24);
    public async Task<Result<ReviewDto>> Handle(EditReviewCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        Domain.Review.Review? review = await context.Review
            .Include(r => r.User).Include(r => r.Order)
            .FirstOrDefaultAsync(r => r.Id == command.ReviewId, cancellationToken);

        if (review == null)
        {
            return Result.Failure<ReviewDto>(Domain.Common.CommonErrors.CustomErrorMessage("Review not found!"));
        }

        if (review.UserId != userId)
        {
            return Result.Failure<ReviewDto>(Domain.Common.CommonErrors.CustomErrorMessage("Review not found!"));
        }
        DateTime? editDeadline = review.CreatedAt?.Add(EditWindow);

        if (DateTime.UtcNow > editDeadline)
        {
            return Result.Failure<ReviewDto>(Domain.Common.CommonErrors.CustomErrorMessage($"Reviews can only be edited within 24 hours of submission. " +
                $"The edit window closed at {editDeadline:f} UTC."));
        }

        bool ratingChanged = review.Rating != command.Rating;

        if (ratingChanged)
        {
            review.Rating = command.Rating;
            review.Comment = command.Comment;
            review.UpdatedAt = dateTimeProvider.UtcNow;
            review.UpdatedBy = userId.ToString();
        }

        review.Raise(new ReviewRatingUpdateEvent(review.Order.RestaurantId));

        await context.SaveChangesAsync(cancellationToken);
        return (ReviewDto)review;
    }
}
