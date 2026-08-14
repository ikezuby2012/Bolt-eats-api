using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetRestaurantById;

internal sealed class GetRestaurantByIdQueryHandler(IApplicationDbContext context) : IQueryHandler<GetRestaurantByIdQuery, RestaurantDto>
{
    public async Task<Result<RestaurantDto>> Handle(GetRestaurantByIdQuery query, CancellationToken cancellationToken)
    {
        Domain.Restaurant.Restaurant? restaurant = await context.Restaurants.Include(x => x.Addresses).FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

        if (restaurant is null)
        {
            return Result.Failure<RestaurantDto>(Domain.Common.CommonErrors.CustomErrorMessage("Restaurant Not found!"));
        }

        return Result.Success((RestaurantDto)restaurant);
    }
}
