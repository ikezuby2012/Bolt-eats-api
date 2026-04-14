using Application.Abstractions.Messaging;
using Application.Reviews.Dto;
using SharedKernel;

namespace Application.Restaurant.GetRestaurantReviews;

public sealed record GetRestaurantReviewsQuery(
    Guid RestaurantId,
    int PageSize = 1000,
    int pageNumber = 1
    ) : IQuery<PaginatedResult<ReviewDto>>;
