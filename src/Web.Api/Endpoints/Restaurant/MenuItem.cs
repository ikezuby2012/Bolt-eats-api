using Application.Abstractions.Messaging;
using Application.Abstractions.Services.UploadMedia;
using Application.Abstractions.Services.UploadResult;
using Application.Restaurant.AddMenuItem;
using Application.Restaurant.DeleteMenuItem;
using Application.Restaurant.Dto;
using Application.Restaurant.GetNearbyRestaurantMenuItem;
using Application.Restaurant.GetRelatedMenuItemsByRestaurant;
using Application.Restaurant.GetRestaurantMenu;
using Application.Restaurant.GetRestaurantMenuItemDetails;
using Application.Restaurant.UpdateMenuItem;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Restaurant;

public class MenuItem : IEndpoint
{
    internal sealed class AddMenuItemRequest
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int? Calories { get; set; }
        public int PrepTimeMin { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsPopular { get; set; }
        public int SortOrder { get; set; }
        public IFormFile? Image { get; set; }
    }

    internal sealed class AddMenuItemRequest1
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int? Calories { get; set; }
        public int PrepTimeMin { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsPopular { get; set; }
        public int SortOrder { get; set; }
        public string? ImageUrl { get; set; }
    }

    internal sealed record UpdateMenuItemRequest(
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    string? ImageLink,
    int? Calories,
    int PrepTimeMin,
    bool IsAvailable,
    bool IsPopular,
    int SortOrder
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("restaurant/{Id:guid}/menu-item-dump", async (Guid Id, [FromBody] AddMenuItemRequest1 body, ICommandHandler<AddMenuItemCommand, MenuItemDto> handler, IImageUploadService imageService, CancellationToken cancellationToken) =>
        {

            var command = new AddMenuItemCommand(Id, body.CategoryId, body.Name, body.Description, body.Price, body.DiscountPrice, body.ImageUrl, body.Calories, body.PrepTimeMin, body.IsAvailable, body.IsPopular, body.SortOrder);

            Result<MenuItemDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<MenuItemDto>.Success(value, "Restaurant Menu item added successfully")), error =>
            {
                return CustomResults.Problem(error);
            });
        }).WithTags(Tags.Restaurant).RequireAuthorization().ExcludeFromDescription();

        app.MapPost("restaurant/{Id:guid}/menu-item", async (Guid Id, [FromForm] AddMenuItemRequest body, ICommandHandler<AddMenuItemCommand, MenuItemDto> handler, IImageUploadService imageService, CancellationToken cancellationToken) =>
        {
            string? imageUrl = null;
            string? imagePublicId = null;

            if (body.Image is not null)
            {
                await using Stream imageStream = body.Image.OpenReadStream();

                ImageUploadResult uploadResult = await imageService.UploadAsync(
                    stream: imageStream,
                    fileName: body.Image.FileName,
                    folder: UploadFolders.MenuItems,
                    options: new ImageUploadOptions(
                        MaxWidthPx: 800,
                        MaxHeightPx: 800,
                        Quality: 85,
                        Format: "webp",
                        GenerateThumbnail: true,
                        ThumbnailSizePx: 200),
                    cancellationToken: cancellationToken);

                if (!uploadResult.IsSuccess)
                {
                    return Results.BadRequest(ApiResponse<string>.Error($"Menu item image upload failed: {uploadResult.Error}"));
                }

                imageUrl = uploadResult.Link;
                imagePublicId = uploadResult.PublicId;
            }
            var command = new AddMenuItemCommand(Id, body.CategoryId, body.Name, body.Description, body.Price, body.DiscountPrice, imageUrl, body.Calories, body.PrepTimeMin, body.IsAvailable, body.IsPopular, body.SortOrder);

            Result<MenuItemDto> result = await handler.Handle(command, cancellationToken);

            if (result.IsFailure && imagePublicId is not null)
            {
                await imageService.DeleteAsync(
                    imagePublicId,
                    cancellationToken);
            }

            return result.Match(value => Results.Ok(ApiResponse<MenuItemDto>.Success(value, "Restaurant Menu item added successfully")), error =>
            {
                return CustomResults.Problem(error);
            });
        }).WithTags(Tags.Restaurant).RequireAuthorization().DisableAntiforgery();

        app.MapGet("restaurant/{Id:guid}/menu-item", async (Guid Id, IQueryHandler<GetRestaurantMenuQuery, IEnumerable<MenuItemDto>> handler, CancellationToken cancellationToken) =>
        {
            var command = new GetRestaurantMenuQuery(Id);

            Result<IEnumerable<MenuItemDto>> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<IEnumerable<MenuItemDto>>.Success(value, "All Restaurant Menu item")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapPut("restaurant/{Id:guid}/menu-item/{Mid:guid}", async (Guid Id, Guid Mid, [FromBody] UpdateMenuItemRequest req, ICommandHandler<UpdateMenuItemCommand, MenuItemDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new UpdateMenuItemCommand(Mid, Id, req.CategoryId, req.Name, req.Description, req.Price, req.DiscountPrice, req.ImageLink, req.Calories, req.PrepTimeMin, req.IsAvailable, req.IsPopular, req.SortOrder);

            Result<MenuItemDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<MenuItemDto>.Success(value, "Restaurant Menu item Updated Successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapDelete("restaurant/menu-item/{Id:guid}", async (Guid Id, ICommandHandler<DeleteMenuItemCommand> handler, CancellationToken cancellationToken) =>
        {
            var command = new DeleteMenuItemCommand(Id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                 () => Results.NoContent(),
                 error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapGet("restaurant/nearby/menu-item", async (IQueryHandler<GetNearbyRestaurantMenuItemsQuery, PaginatedResult<NearbyMenuItemDto>> handler,
                CancellationToken cancellationToken,
                [FromQuery] double lat = 0,
                [FromQuery] double lng = 0,
                [FromQuery] double radiusKm = 5,
                [FromQuery] Guid? categoryId = null,
                [FromQuery] string? search = null,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 20) =>
        {
            var query = new GetNearbyRestaurantMenuItemsQuery(lat, lng, radiusKm, categoryId, search, pageNumber, pageSize);

            Result<PaginatedResult<NearbyMenuItemDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaginatedResult<NearbyMenuItemDto>>.Success(value, "All Nearby Restaurant Menu item")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).WithName("GetNearbyRestaurantMenuItems").Produces<ApiResponse<PaginatedResult<NearbyMenuItemDto>>>();

        // GET restaurant/menu-item/{id:guid}
        app.MapGet("restaurant/menu-item/{id:guid}", async (
            Guid id,
            IQueryHandler<GetRestaurantMenuItemDetailsQuery, MenuItemDetailDto> handler,
            CancellationToken ct) =>
        {

            var query = new GetRestaurantMenuItemDetailsQuery(id);

            Result<MenuItemDetailDto> result = await handler.Handle(query, ct);

            return result.Match(value => Results.Ok(ApiResponse<MenuItemDetailDto>.Success(value, "Menu item Details Information")), error => CustomResults.Problem(error));
        })
        .WithName("GetMenuItemDetail")
        .WithTags(Tags.Restaurant)
        .Produces<ApiResponse<MenuItemDetailDto>>()
        .Produces(404);

        // GET restaurant/{id:guid}/menu-item/related
        app.MapGet("restaurant/{id:guid}/menu-item/related", async (
            Guid id,
            IQueryHandler<GetRelatedMenuItemsByRestaurantQuery, IReadOnlyList<RelatedMenuItemDto>> handler,
            CancellationToken ct,
            [FromQuery] Guid excludeMenuItemId,
            [FromQuery] int limit = 10) =>
        {
            Result<IReadOnlyList<RelatedMenuItemDto>> result = await handler.Handle(
                new GetRelatedMenuItemsByRestaurantQuery(id, excludeMenuItemId, limit), ct);

            return result.Match(
                value => Results.Ok(
                    ApiResponse<IReadOnlyList<RelatedMenuItemDto>>.Success(value)),
                error => CustomResults.Problem(error));
        })
        .WithName("GetRelatedMenuItemsByRestaurant")
        .WithTags(Tags.Restaurant)
        .Produces<ApiResponse<IReadOnlyList<RelatedMenuItemDto>>>()
        .Produces(404);
    }
}
