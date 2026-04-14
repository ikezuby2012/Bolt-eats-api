using Application.Abstractions.Messaging;
using Application.Restaurant.AddMenuCategory;
using Application.Restaurant.DeleteMenuCategory;
using Application.Restaurant.Dto;
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
    }
}
