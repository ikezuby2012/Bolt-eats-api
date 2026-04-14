using System.Collections;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using SharedKernel;

namespace Application.Restaurant.GetAllRestaurant;

public sealed record GetAllRestaurantQuery(
    int PageSize = 1000,
    int pageNumber = 1,
    DateTime? DateFrom = null,
    DateTime? DateTo = null, bool? IsActive = null) : IQuery<PaginatedResult<RestaurantDto>>;
