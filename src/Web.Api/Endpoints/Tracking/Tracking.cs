using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Tracking.Dto;
using Application.Tracking.GetOrderTracking;
using Application.Tracking.GetOrderTrackingSnapshot;
using Application.Tracking.PushRiderLocation;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tracking;

public class Tracking : IEndpoint
{
    internal sealed record PushLocationRequest(Guid OrderId, double Latitude, double Longitude, double? Accuracy, double? Bearing, double? Speed);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("tracking").WithTags(Tags.Tracking).RequireAuthorization();

        group.MapGet("/order/{orderId:guid}/snapshot", async (
            Guid orderId, [FromServices] IQueryHandler<GetOrderTrackingSnapshotQuery, OrderTrackingSnapshotDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetOrderTrackingSnapshotQuery(orderId);

            Result<OrderTrackingSnapshotDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<OrderTrackingSnapshotDto>.Success(value, "Order Tracking Snapshots")), error => CustomResults.Problem(error));
        })
        .WithName("GetOrderTrackingSnapshot")
        .Produces<OrderTrackingSnapshotDto>()
        .Produces<ProblemDetails>(404);

        group.MapPost("/location", async ([FromBody] PushLocationRequest body, IUserContext userContext, [FromServices] ICommandHandler<PushRiderLocationCommand> handler, CancellationToken cancellationToken) =>
        {
            var command = new PushRiderLocationCommand(userContext.UserId, OrderId: body.OrderId,
                Latitude: body.Latitude,
                Longitude: body.Longitude,
                Accuracy: body.Accuracy,
                Bearing: body.Bearing,
                Speed: body.Speed);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                 () => Results.NoContent(),
                 error => CustomResults.Problem(error));
        })
       .WithName("PushRiderLocation")
       .RequireAuthorization("Rider")
       .Produces(204)
       .Produces<ProblemDetails>(400);

        // Endpoint — add to tracking group

        // GET /tracking/order/{id}
        group.MapGet("order/{id:guid}", async (
            Guid id,
            IUserContext ctx,
            IQueryHandler<GetOrderTrackingQuery, OrderTrackingDto> handler,
            CancellationToken ct) =>
        {
            Result<OrderTrackingDto> result = await handler.Handle(
                new GetOrderTrackingQuery(id, ctx.UserId), ct);

            return result.Match(
                value => Results.Ok(
                    ApiResponse<OrderTrackingDto>.Success(value)),
                error => CustomResults.Problem(error));
        })
        .WithName("GetOrderTracking")
        .WithTags(Tags.Tracking)
        .RequireAuthorization()
        .Produces<ApiResponse<OrderTrackingDto>>()
        .Produces(404);
    }

}
