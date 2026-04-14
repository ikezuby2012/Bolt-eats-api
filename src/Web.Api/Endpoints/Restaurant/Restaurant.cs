using Application.Abstractions.Messaging;
using Application.Restaurant.CreateRestaurant;
using Application.Restaurant.DeleteRestaurant;
using Application.Restaurant.Dto;
using Application.Restaurant.GetAllRestaurant;
using Application.Restaurant.GetFeaturedRestaurant;
using Application.Restaurant.GetNearbyRestaurant;
using Application.Restaurant.GetRestaurantById;
using Application.Restaurant.ToggleStatusRestaurant;
using Application.Restaurant.UpdateRestaurantInfo;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Restaurant;

public class Restaurant : IEndpoint
{
    internal sealed record ToggleStatusReq(bool IsOpen);
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("restaurant", async ([AsParameters] GetAllRestaurantQuery param, IQueryHandler<GetAllRestaurantQuery, PaginatedResult<RestaurantDto>> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetAllRestaurantQuery(param.PageSize, param.pageNumber, param.DateFrom, param.DateTo, param.IsActive);

            Result<PaginatedResult<RestaurantDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaginatedResult<RestaurantDto>>.Success(value, "All Restaurants")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapGet("restaurant/{Id:Guid}", async (Guid Id, IQueryHandler<GetRestaurantByIdQuery, RestaurantDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetRestaurantByIdQuery(Id);

            Result<RestaurantDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<RestaurantDto>.Success(value, "Restaurant fetched successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapPost("restaurant", async ([FromBody] CreateRestaurantCommand body, ICommandHandler<CreateRestaurantCommand, RestaurantDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new CreateRestaurantCommand(body.Name, body.Description, body.PhoneNumber, body.Email, body.LogoLink, body.BannerLink, body.DeliveryFeeMin, body.DeliveryFeeMax, body.MinOrderAmount, body.EstDeliveryMin, body.EstDeliveryMax, body.CompanyPartner, body.Address);

            Result<RestaurantDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<RestaurantDto>.Success(value, "Restaurant Created Successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapPatch("restaurant/{Id:guid}", async (Guid Id, [FromBody] UpdateRestaurantCommand body, ICommandHandler<UpdateRestaurantCommand, RestaurantDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new UpdateRestaurantCommand(Id, body.Name, body.Description, body.PhoneNumber, body.Email, body.LogoLink, body.BannerLink, body.DeliveryFeeMin, body.DeliveryFeeMax, body.MinOrderAmount, body.EstDeliveryMin, body.EstDeliveryMax, body.CompanyPartner, body.Address);

            Result<RestaurantDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<RestaurantDto>.Success(value, "Restaurant Updated Successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapDelete("restaurant/{Id:guid}", async (Guid Id, ICommandHandler<DeleteRestaurantCommand> handler, CancellationToken cancellationToken) =>
        {
            var command = new DeleteRestaurantCommand(Id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                 () => Results.NoContent(),
                 error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapGet("restaurants/nearby", async ([AsParameters] GetNearbyRestaurantQuery param, IQueryHandler<GetNearbyRestaurantQuery, PaginatedResult<RestaurantDto>> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetNearbyRestaurantQuery(param.lat, param.lng, param.RadiusKm, param.PageSize, param.pageNumber);

            Result<PaginatedResult<RestaurantDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaginatedResult<RestaurantDto>>.Success(value, "All Nearby Restaurants")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization().WithName("GetNearbyRestaurants");

        app.MapGet("restaurants/featured", async ([AsParameters] GetFeaturedRestaurantQuery param, IQueryHandler<GetFeaturedRestaurantQuery, PaginatedResult<RestaurantDto>> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetFeaturedRestaurantQuery();

            Result<PaginatedResult<RestaurantDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaginatedResult<RestaurantDto>>.Success(value, "All Featured Restaurants")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization().WithName("GetFeaturedRestaurants");

        app.MapPatch("restaurant/{Id:guid}/toggle-status", async (Guid Id, [FromBody] ToggleStatusReq body, ICommandHandler<ToggleStatusRestaurantCommand, RestaurantDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new ToggleStatusRestaurantCommand(Id, body.IsOpen);

            Result<RestaurantDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<RestaurantDto>.Success(value, "Restaurant Toggle Status Updated Successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();
    }
}
