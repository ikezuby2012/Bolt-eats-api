using System.Threading;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Application.Restaurant.GetAfrricanCuisine;
using Application.Restaurant.GetBestChoice;
using Application.Restaurant.GetCheapDelivery;
using Application.Restaurant.GetDrinksAndSmoothies;
using Application.Restaurant.GetInternationalBites;
using Application.Restaurant.GetProtein;
using Application.Restaurant.GetQuickEats;
using Application.Restaurant.GetRecentOffer;
using Application.Restaurant.GetRiceDishes;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Restaurant;

public class Home : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/home").WithTags(Tags.Restaurant);

        group.MapGet("/offers", async (
            IQueryHandler<GetRecentOfferQuery, IReadOnlyList<HomeMenuItemDto>> handler, CancellationToken cancellationToken,
            [FromQuery] int limit = 10) =>
        {
            var query = new GetRecentOfferQuery(limit);

            Result<IReadOnlyList<HomeMenuItemDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<IReadOnlyList<HomeMenuItemDto>>.Success(value, "All Today Offers")), error => CustomResults.Problem(error));
        })
        .WithName("GetTodaysOffers")
        .Produces<IReadOnlyList<HomeMenuItemDto>>();

        group.MapGet("/quick-eats", async (
            IQueryHandler<GetQuickEatsQuery, IReadOnlyList<HomeMenuItemDto>> handler, CancellationToken cancellationToken,
            [FromQuery] int limit = 10) =>
        {
            var query = new GetQuickEatsQuery(limit);

            Result<IReadOnlyList<HomeMenuItemDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<IReadOnlyList<HomeMenuItemDto>>.Success(value, "Quick Eats Section")), error => CustomResults.Problem(error));
        })
        .WithName("GetQuickEats")
        .Produces<IReadOnlyList<HomeMenuItemDto>>();

        group.MapGet("/africa-cuisine", async (
            IQueryHandler<GetAfricanCuisineQuery, IReadOnlyList<AfricanCuisineItemDto>> handler, CancellationToken cancellationToken,
            [FromQuery] int limit = 12) =>
        {
            var query = new GetAfricanCuisineQuery(limit);

            Result<IReadOnlyList<AfricanCuisineItemDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<IReadOnlyList<AfricanCuisineItemDto>>.Success(value, "Quick Eats Section")), error => CustomResults.Problem(error));
        })
        .WithName("GetGroceries")
        .Produces<IReadOnlyList<AfricanCuisineItemDto>>();

        group.MapGet("/cheap-delivery", async (
             IQueryHandler<GetCheapDeliveryQuery, IReadOnlyList<HomeMenuItemDto>> handler, CancellationToken cancellationToken,
            [FromQuery] decimal maxFee = 1000,
            [FromQuery] int limit = 10) =>
        {
            var query = new GetCheapDeliveryQuery(maxFee, limit);

            Result<IReadOnlyList<HomeMenuItemDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<IReadOnlyList<HomeMenuItemDto>>.Success(value, "Get Cheap Delivery fee Items")), error => CustomResults.Problem(error));
        })
        .WithName("GetCheapDelivery")
        .Produces<IReadOnlyList<HomeMenuItemDto>>();

        group.MapGet("/best-choice", async (
            IQueryHandler<GetBestChoiceQuery, IReadOnlyList<HomeMenuItemDto>> handler, CancellationToken ct,
            [FromQuery] double minRating = 3.5,
            [FromQuery] int limit = 10) =>
        {
            var query = new GetBestChoiceQuery(minRating, limit);

            Result<IReadOnlyList<HomeMenuItemDto>> result = await handler.Handle(query, ct);

            return result.Match(value => Results.Ok(ApiResponse<IReadOnlyList<HomeMenuItemDto>>.Success(value, "User Popular Best Choice items")), error => CustomResults.Problem(error));
        })
        .WithName("GetBestChoice")
        .Produces<IReadOnlyList<HomeMenuItemDto>>();

        group.MapGet("/rice-dishes", async (
           IQueryHandler<GetRiceDishesQuery, IReadOnlyList<HomeSectionItemDto>> handler, CancellationToken ct,
           [FromQuery] int limit = 10) =>
        {
            var query = new GetRiceDishesQuery(limit);

            Result<IReadOnlyList<HomeSectionItemDto>> result = await handler.Handle(query, ct);

            return result.Match(value => Results.Ok(ApiResponse<IReadOnlyList<HomeSectionItemDto>>.Success(value, "Popular Rice and Dishes")), error => CustomResults.Problem(error));
        })
       .WithName("GetRiceDishes")
       .Produces<IReadOnlyList<HomeMenuItemDto>>();

        group.MapGet("/protein", async (
           IQueryHandler<GetProteinFixQuery, IReadOnlyList<HomeSectionItemDto>> handler, CancellationToken ct,
           [FromQuery] int limit = 10) =>
        {
            var query = new GetProteinFixQuery(limit);

            Result<IReadOnlyList<HomeSectionItemDto>> result = await handler.Handle(query, ct);

            return result.Match(value => Results.Ok(ApiResponse<IReadOnlyList<HomeSectionItemDto>>.Success(value, "Popular Proteins")), error => CustomResults.Problem(error));
        })
       .WithName("GetProtein")
       .Produces<IReadOnlyList<HomeMenuItemDto>>();

        group.MapGet("/international-cuisine", async (
           IQueryHandler<GetInternationalBitesQuery, IReadOnlyList<HomeSectionItemDto>> handler, CancellationToken ct,
           [FromQuery] int limit = 10) =>
        {
            var query = new GetInternationalBitesQuery(limit);

            Result<IReadOnlyList<HomeSectionItemDto>> result = await handler.Handle(query, ct);

            return result.Match(value => Results.Ok(ApiResponse<IReadOnlyList<HomeSectionItemDto>>.Success(value, "Popular International Dishes")), error => CustomResults.Problem(error));
        })
       .WithName("InternationalDishes")
       .Produces<IReadOnlyList<HomeMenuItemDto>>();

        group.MapGet("/drinks-smoothies", async (
            IQueryHandler<GetDrinksAndSmoothiesQuery, IReadOnlyList<HomeSectionItemDto>> handler, CancellationToken ct,
           [FromQuery] int limit = 10) =>
        {
            var query = new GetDrinksAndSmoothiesQuery(limit);

            Result<IReadOnlyList<HomeSectionItemDto>> result = await handler.Handle(query, ct);

            return result.Match(value => Results.Ok(ApiResponse<IReadOnlyList<HomeSectionItemDto>>.Success(value, "Drinks And Smoothies")), error => CustomResults.Problem(error));
        })
       .WithName("DrinksAndSmoothies")
       .Produces<IReadOnlyList<HomeMenuItemDto>>();
    }
}
