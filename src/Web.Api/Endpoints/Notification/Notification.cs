using Application.Abstractions.Messaging;
using Application.Notification.DeleteNotification;
using Application.Notification.Dto;
using Application.Notification.GetNotification;
using Application.Notification.MarkNotificationAsRead;
using Application.Notification.RegisterDeviceToken;
using Application.Notification.UnregisterDeviceToken;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Notification;

public class Notification : IEndpoint
{
    internal sealed record RegisterTokenRequest(string Token, string Platform);
    internal sealed record UnregisterTokenRequest(string Token);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup("/notifications")
            .WithTags(Tags.Notification)
            .RequireAuthorization();

        group.MapGet("/", async (
            [FromServices] IQueryHandler<GetNotificationQuery, PaginatedResult<NotificationDto>> handler,
            CancellationToken cancellationToken,
            [FromQuery] bool unreadOnly = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var query = new GetNotificationQuery(unreadOnly, page, pageSize);

            Result<PaginatedResult<NotificationDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaginatedResult<NotificationDto>>.Success(value, "My Notifications")), error => CustomResults.Problem(error));
        })
        .WithName("GetNotifications")
        .Produces<PaginatedResult<NotificationDto>>();

        // PUT /{id}/read
        group.MapPut("/{id:guid}/read", async (
            Guid id,
            ICommandHandler<MarkNotificationAsReadCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new MarkNotificationAsReadCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                 () => Results.Ok(),
                 error => CustomResults.Problem(error));
        })
        .WithName("MarkNotificationRead")
        .Produces(204)
        .Produces<ProblemDetails>(404);

        // PUT /read-all
        // NOTE: must be registered before /{id}/read — otherwise
        // the router matches "read-all" as a Guid parameter and 404s
        group.MapPut("/read-all", async (
            ICommandHandler<MarkNotificationAsReadCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new MarkNotificationAsReadCommand();

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                 () => Results.Ok(),
                 error => CustomResults.Problem(error));
        })
        .WithName("MarkAllNotificationsRead")
        .Produces(204);

        // DELETE /{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteNotificationCommand> handler,
             CancellationToken cancellationToken) =>
        {
            var command = new DeleteNotificationCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                 () => Results.NoContent(),
                 error => CustomResults.Problem(error));
        })
        .WithName("DeleteNotification")
        .Produces(204)
        .Produces<ProblemDetails>(404);

        // POST /device-token
        group.MapPost("/device-token", async (
           [FromBody] RegisterTokenRequest body,
           [FromServices] ICommandHandler<RegisterDeviceTokenCommand> handler, CancellationToken cancellationToken) =>
        {
            var command = new RegisterDeviceTokenCommand(body.Token, body.Platform);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                 () => Results.Created(),
                 error => CustomResults.Problem(error));
        })
        .WithName("RegisterDeviceToken")
        .Produces(204);

        // DELETE /device-token
        group.MapDelete("/device-token", async (
            [FromBody] UnregisterTokenRequest body,
            [FromServices] ICommandHandler<UnregisterDeviceTokenCommand> handler, CancellationToken cancellationToken) =>
        {
            var command = new UnregisterDeviceTokenCommand(body.Token);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                 () => Results.NoContent(),
                 error => CustomResults.Problem(error));
        })
        .WithName("UnregisterDeviceToken")
        .Produces(204);
    }
}
