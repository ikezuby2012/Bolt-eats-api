using Application.Abstractions.Messaging;
using Application.Restaurant.AddMenuItem;
using Application.Restaurant.DeleteMenuItem;
using Application.Restaurant.Dto;
using Application.Restaurant.GetRestaurantMenu;
using Application.Restaurant.UpdateMenuItem;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Restaurant;

public class MenuItem : IEndpoint
{
    internal sealed record AddMenuItemRequest(
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
    int SortOrder);

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
        app.MapPost("restaurant/{Id:guid}/menu-item", async (Guid Id, [FromBody] AddMenuItemRequest body, ICommandHandler<AddMenuItemCommand, MenuItemDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new AddMenuItemCommand(Id, body.CategoryId, body.Name, body.Description, body.Price, body.DiscountPrice, body.ImageLink, body.Calories, body.PrepTimeMin, body.IsAvailable, body.IsPopular, body.SortOrder);

            Result<MenuItemDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<MenuItemDto>.Success(value, "Restaurant Menu item added successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapGet("restaurant/{Id:guid}/menu-item", async (Guid Id, IQueryHandler<GetRestaurantMenuQuery, IEnumerable<CategoryDto>> handler, CancellationToken cancellationToken) =>
        {
            var command = new GetRestaurantMenuQuery(Id);

            Result<IEnumerable<CategoryDto>> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<IEnumerable<CategoryDto>>.Success(value, "All Restaurant Menu item")), error => CustomResults.Problem(error));
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
    }
}
