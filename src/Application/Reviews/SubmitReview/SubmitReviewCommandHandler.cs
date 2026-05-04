using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Reviews.Dto;
using Domain.Order;
using Domain.Review;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reviews.SubmitReview;

internal sealed class SubmitReviewCommandHandler(IApplicationDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<SubmitReviewCommand, ReviewDto>
{

    public async Task<Result<ReviewDto>> Handle(SubmitReviewCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        Domain.Order.Order? order = await context.Order
            .Include(o => o.Restaurant)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

        if (order == null)
        {
            return Result.Failure<ReviewDto>(Domain.Common.CommonErrors.CustomErrorMessage("Order not found!"));
        }

        if (order.CustomerId != userId)
        {
            return Result.Failure<ReviewDto>(Domain.Common.CommonErrors.CustomErrorMessage("Order not found!"));
        }

        // Rule: order must be Delivered before a review can be submitted
        if (order.OrderStatusId == EOrderStatus.Delivered.Id)
        {
            return Result.Failure<ReviewDto>(Domain.Common.CommonErrors.CustomErrorMessage("A review can only be submitted for a delivered order."));
        }

        bool alreadyReviewed = await context.Review
            //.IgnoreQueryFilters()
            .AnyAsync(r => r.OrderId == command.OrderId, cancellationToken);

        if (alreadyReviewed)
        {
            return Result.Failure<ReviewDto>(Domain.Common.CommonErrors.CustomErrorMessage("You have already submitted a review for this order."));
        }

        var review = new Review
        {
            OrderId = command.OrderId,
            RestaurantId = order.RestaurantId,
            UserId = userId,
            Rating = command.Rating,
            Comment = command.Comment,
            CreatedAt = dateTimeProvider.UtcNow,
            CreatedBy = userId.ToString(),
        };

        review.Raise(new ReviewRatingUpdateEvent(order.RestaurantId));

        await context.Review.AddAsync(review, cancellationToken);

        return (ReviewDto)review;
    }
}
