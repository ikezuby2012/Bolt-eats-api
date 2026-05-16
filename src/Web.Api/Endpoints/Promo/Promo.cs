using Application.Abstractions.Messaging;
using Application.Promo.CreatePromoCode;
using Application.Promo.DeactivatePromoCode;
using Application.Promo.Dto;
using Application.Promo.GetPromoCode;
using Application.Promo.UpdatePromoCode;
using Application.Promo.ValidatePromoCode;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Promo;

public class Promo : IEndpoint
{
    internal sealed record ValidatePromoRequest(string Code);
    internal sealed record UpdatePromoCodeRequest(
        string? Description,
        decimal? MinOrderAmount,
        decimal? MaxDiscountCap,
        int? UsageLimitTotal,
        int? UsageLimitPerUser,
        DateTime? StartsAt,
        DateTime? ExpiresAt);

    internal sealed record CreatePromoCodeRequest(
    string Code,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    decimal? MaxDiscountCap,
    Guid? RestaurantId,
    int? UsageLimitTotal,
    int? UsageLimitPerUser,
    DateTime StartsAt,
    DateTime ExpiresAt
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/promos").WithTags(Tags.Promo);

        group.MapPost("/validate", async (
          [FromBody] ValidatePromoRequest body, [FromServices] ICommandHandler<ValidatePromoCodeCommand, PromoValidationResultDto> handler, CancellationToken cancellationToken
           ) =>
        {
            var command = new ValidatePromoCodeCommand(body.Code);

            Result<PromoValidationResultDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PromoValidationResultDto>.Success(value, "Promo Code validated Successfully")), error => CustomResults.Problem(error));
        })
       .WithName("ValidatePromoCode")
       .RequireAuthorization()
       .Produces<PromoValidationResultDto>();

        // GET / — Admin
        group.MapGet("/", async (
            [FromServices] IQueryHandler<GetPromoCodeQuery, PaginatedResult<PromoCodeDto>> handler,
            CancellationToken cancellationToken,
            [FromQuery] bool? activeOnly = null,
            [FromQuery] Guid? restaurantId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var query = new GetPromoCodeQuery(activeOnly, restaurantId, page, pageSize);

            Result<PaginatedResult<PromoCodeDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaginatedResult<PromoCodeDto>>.Success(value, "All Promo Codes")), error => CustomResults.Problem(error));
        })
        .WithName("GetPromoCodes")
        .RequireAuthorization()
        .Produces<PaginatedResult<PromoCodeDto>>();

        // POST / — Admin
        group.MapPost("/", async (
            [FromBody] CreatePromoCodeRequest request,
            [FromServices] ICommandHandler<CreatePromoCodeCommand, PromoCodeDto> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreatePromoCodeCommand(request.Code, request.Description, request.DiscountType, request.DiscountValue, request.MinOrderAmount, request.MaxDiscountCap, request.RestaurantId, request.UsageLimitTotal, request.UsageLimitPerUser, request.StartsAt, request.ExpiresAt);

            Result<PromoCodeDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PromoCodeDto>.Success(value, "Promo Code Created Successfully")), error => CustomResults.Problem(error));
        })
        .WithName("CreatePromoCode")
        .RequireAuthorization()
        .Produces<PromoCodeDto>(201)
        .Produces<ProblemDetails>(409);

        // PUT /{id} — Admin
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdatePromoCodeRequest body,
            [FromServices] ICommandHandler<UpdatePromoCodeCommand, PromoCodeDto> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdatePromoCodeCommand(
                id,
                body.Description,
                body.MinOrderAmount,
                body.MaxDiscountCap,
                body.UsageLimitTotal,
                body.UsageLimitPerUser,
                body.StartsAt,
                body.ExpiresAt);

            Result<PromoCodeDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PromoCodeDto>.Success(value, "Promo Code Updated Successfully")), error => CustomResults.Problem(error));
        })
        .WithName("UpdatePromoCode")
        .RequireAuthorization()
        .Produces<PromoCodeDto>()
        .Produces<ProblemDetails>(404);

        // DELETE /{id} — Admin (soft deactivate)
        group.MapDelete("/{id:guid}", async (
            Guid id,
            [FromServices] ICommandHandler<DeactivatePromoCodeCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeactivatePromoCodeCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                 () => Results.NoContent(),
                 error => CustomResults.Problem(error));
        })
        .WithName("DeactivatePromoCode")
        .RequireAuthorization()
        .Produces(204)
        .Produces<ProblemDetails>(404);
    }
}
