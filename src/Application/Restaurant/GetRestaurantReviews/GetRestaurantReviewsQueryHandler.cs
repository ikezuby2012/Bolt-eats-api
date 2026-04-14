using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Reviews.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetRestaurantReviews;

internal sealed class GetRestaurantReviewsQueryHandler(IApplicationDbContext context) : IQueryHandler<GetRestaurantReviewsQuery, PaginatedResult<ReviewDto>>
{
    public async Task<Result<PaginatedResult<ReviewDto>>> Handle(GetRestaurantReviewsQuery query, CancellationToken cancellationToken)
    {
        bool restaurantExist = await context.Restaurants.AnyAsync(x => x.Id == query.RestaurantId, cancellationToken);

        if (restaurantExist)
        {
            return Result.Failure<PaginatedResult<ReviewDto>>(Domain.Common.CommonErrors.CustomErrorMessage("Restaurant Not found!"));
        }

        IOrderedQueryable<Domain.Review.Review> baseQuery = context.Review.AsNoTracking().AsQueryable().Where(x => x.RestaurantId == query.RestaurantId).OrderByDescending(x => x.CreatedAt);

        int totalItems = await baseQuery.CountAsync(cancellationToken);

        List<ReviewDto> items = await baseQuery.Skip((query.pageNumber - 1) * query.PageSize)
            .Select(x => (ReviewDto)x).ToListAsync(cancellationToken);

        return new PaginatedResult<ReviewDto>
        {
            Data = items,
            TotalItems = totalItems,
            PageSize = query.PageSize,
            PageNumber = query.pageNumber,
        };
    }
}
