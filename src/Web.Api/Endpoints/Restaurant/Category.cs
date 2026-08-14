using Application.Abstractions.Messaging;
using Application.Restaurant.AddMenuCategory;
using Application.Restaurant.DeleteMenuCategory;
using Application.Restaurant.Dto;
using Application.Restaurant.GetCommonCategory;
using Application.Restaurant.GetNearbyRestaurantCategories;
using Application.Restaurant.GetRelatedMenuItemsByCategory;
using Application.Restaurant.GetRestaurantMenuCategories;
using Application.Restaurant.UpdateCategory;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Restaurant;

public class Category : IEndpoint
{
    internal sealed record AddMenuCategoryRequest(string Name, int SortOrder);
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("restaurant/{Id:guid}/categories", async (Guid Id, [FromBody] AddMenuCategoryRequest body, ICommandHandler<AddMenuCategoryCommand, CategoryDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new AddMenuCategoryCommand(Id, body.Name, body.SortOrder);

            Result<CategoryDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<CategoryDto>.Success(value, "Restaurant Category added successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapPut("restaurant/{Id:guid}/categories/{Cid:guid}", async (Guid Id, Guid Cid, [FromBody] AddMenuCategoryRequest req, ICommandHandler<UpdateCategoryCommand, CategoryDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new UpdateCategoryCommand(Id, Cid, req.Name, req.SortOrder);

            Result<CategoryDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<CategoryDto>.Success(value, "Restaurant Category Updated Successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapDelete("restaurant/categories/{Id:guid}", async (Guid Id, ICommandHandler<DeleteMenuCategoryCommand> handler, CancellationToken cancellationToken) =>
        {
            var command = new DeleteMenuCategoryCommand(Id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                 () => Results.NoContent(),
                 error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapGet("restaurant/common-categories", async (IQueryHandler<GetCommonCategoryQuery, IReadOnlyList<CommonCategoryDto>> handler, CancellationToken cancellationToken, [FromQuery] int limit = 10) =>
        {
            var command = new GetCommonCategoryQuery(limit);

            Result<IReadOnlyList<CommonCategoryDto>> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<IReadOnlyList<CommonCategoryDto>>.Success(value, "Common Restaurant Category added successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant);

        app.MapGet("restaurant/nearby-categories", async (
            IQueryHandler<GetNearbyRestaurantCategoriesQuery, PaginatedResult<NearbyCategoryDto>> handler,
            CancellationToken cancellationToken,
            [FromQuery] double lat = 0,
            [FromQuery] double lng = 0,
            [FromQuery] double radiusKm = 5,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20
            ) =>
        {
            var query = new GetNearbyRestaurantCategoriesQuery(lat, lng, radiusKm, pageNumber, pageSize);

            Result<PaginatedResult<NearbyCategoryDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaginatedResult<NearbyCategoryDto>>.Success(value, "Nearby Restaurant Category added successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).WithName("GetNearbyRestaurantCategories").Produces<ApiResponse<PaginatedResult<NearbyCategoryDto>>>();

        // the router matching "related" or "category" as a Guid parameter
        app.MapGet("restaurant/menu-item/related/category", async (
            IQueryHandler<GetRelatedMenuItemsByCategoryQuery, IReadOnlyList<RelatedMenuItemDto>> handler,
            CancellationToken ct,
            [FromQuery] string categoryName,
            [FromQuery] Guid excludeMenuItemId,
            [FromQuery] Guid? excludeRestaurantId = null,
            [FromQuery] int limit = 10) =>
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return Results.BadRequest(
                    ApiResponse<string>.Error("categoryName is required."));
            }

            Result<IReadOnlyList<RelatedMenuItemDto>> result = await handler.Handle(
                new GetRelatedMenuItemsByCategoryQuery(
                    categoryName, excludeMenuItemId, excludeRestaurantId, limit), ct);

            return result.Match(
                value => Results.Ok(
                    ApiResponse<IReadOnlyList<RelatedMenuItemDto>>.Success(value)),
                error => CustomResults.Problem(error));
        })
        .WithName("GetRelatedMenuItemsByCategory")
        .WithTags(Tags.Restaurant)
        .Produces<ApiResponse<IReadOnlyList<RelatedMenuItemDto>>>();

        app.MapGet("restaurant/{id:guid}/menu-categories", async (
            Guid id,
            IQueryHandler<GetRestaurantMenuCategoriesQuery, IReadOnlyList<MenuCategoryDto>> handler,
            CancellationToken ct) =>
                {
                    Result<IReadOnlyList<MenuCategoryDto>> result = await handler.Handle(
                        new GetRestaurantMenuCategoriesQuery(id), ct);

                    return result.Match(
                        value => Results.Ok(
                            ApiResponse<IReadOnlyList<MenuCategoryDto>>.Success(value)),
                        error => CustomResults.Problem(error));
                })
        .WithName("GetRestaurantMenuCategories")
        .WithTags(Tags.Restaurant)
        .Produces<ApiResponse<IReadOnlyList<MenuCategoryDto>>>()
        .Produces(404);
    }
}
