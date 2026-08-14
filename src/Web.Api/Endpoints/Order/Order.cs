using Application.Abstractions.Messaging;
using Application.Orders.AdvanceOrderStatus;
using Application.Orders.AssignRider;
using Application.Orders.CancelOrder;
using Application.Orders.CreateOrder;
using Application.Orders.Dto;
using Application.Orders.GetAllOrdersAdmin;
using Application.Orders.GetOrderById;
using Application.Orders.GetOrderHistory;
using Application.Orders.GetRestaurantOrders;
using Application.Orders.GetRiderActiveOrder;
using Application.Orders.TestAutoAsync;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Order;

public class Order : IEndpoint
{
    internal sealed record CreateOrderRequest(
    Guid? AddressId,
    string? ContactEmail,
    string? contactName,
    string? ContactPhone,
    string? CustomerNotes);

    internal sealed record GetAllOrdersAdminParams(
    string? statusFilter,
    Guid? RestaurantId,
    Guid? CustomerId,
    Guid? RiderId,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 20);

    internal sealed record GetOrderHistoryParams(int Page = 1, int PageSize = 20, DateTime? DateFrom = null, DateTime? DateTo = null);
    internal sealed record GetRestaurantOrdersParams(Guid RestaurantId, string? Status, int Page = 1, int PageSize = 20);
    internal sealed record UpdateOrderStatusRequest(string status);
    internal sealed record AssignRiderRequest(Guid RiderId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("order", async ([FromBody] CreateOrderRequest body, ICommandHandler<CreateOrderCommand, OrderDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new CreateOrderCommand(body.AddressId, body.ContactEmail, body.contactName, body.ContactPhone, body.CustomerNotes);

            Result<OrderDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<OrderDto>.Success(value, "Order Created Successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Order).RequireAuthorization();

        app.MapGet("order", async ([AsParameters] GetAllOrdersAdminParams body, IQueryHandler<GetAllOrdersAdminQuery, PaginatedResult<OrderSummaryDto>> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetAllOrdersAdminQuery(body.statusFilter, body.RestaurantId, body.CustomerId, body.RiderId, body.From, body.To, body.Page, body.PageSize);

            Result<PaginatedResult<OrderSummaryDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaginatedResult<OrderSummaryDto>>.Success(value, "Fetched All Order Successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Order).RequireAuthorization();

        app.MapGet("order/{Id:guid}", async (Guid Id, IQueryHandler<GetOrderByIdQuery, OrderDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetOrderByIdQuery(Id);

            Result<OrderDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<OrderDto>.Success(value, "Order Information")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Order).RequireAuthorization();

        app.MapGet("order/history", async ([AsParameters] GetOrderHistoryParams pms, IQueryHandler<GetOrderHistoryQuery, PaginatedResult<OrderSummaryDto>> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetOrderHistoryQuery(pms.Page, pms.PageSize, pms.DateFrom, pms.DateTo);

            Result<PaginatedResult<OrderSummaryDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaginatedResult<OrderSummaryDto>>.Success(value, "All Orders hsitory")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Order).RequireAuthorization();


        app.MapPost("order/{Id:Guid}/cancel", async (Guid Id, [FromQuery] string? reason, ICommandHandler<CancelOrderCommand, Guid> handler, CancellationToken cancellationToken) =>
        {
            var query = new CancelOrderCommand(Id, reason);

            Result<Guid> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<Guid>.Success(value, "Order Cancelled successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Order).RequireAuthorization();


        app.MapGet("order/restaurant/{Id:guid}", async ([AsParameters] GetRestaurantOrdersParams pms, IQueryHandler<GetRestaurantOrdersQuery, PaginatedResult<OrderSummaryDto>> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetRestaurantOrdersQuery(pms.RestaurantId, pms.Status, pms.Page, pms.PageSize);

            Result<PaginatedResult<OrderSummaryDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaginatedResult<OrderSummaryDto>>.Success(value, "All Restaurant Orders")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Order).RequireAuthorization();

        app.MapPut("order/{Id:Guid}/status", async (Guid Id, [FromBody] UpdateOrderStatusRequest body, ICommandHandler<AdvanceOrderStatusCommand, OrderDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new AdvanceOrderStatusCommand(Id, body.status);

            Result<OrderDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<OrderDto>.Success(value, "Order Updated successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Order).RequireAuthorization();


        app.MapGet("order/rider/active", async (IQueryHandler<GetRiderActiveOrderQuery, OrderDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetRiderActiveOrderQuery();

            Result<OrderDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<OrderDto>.Success(value, "Rider Active Orders")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Order).RequireAuthorization();

        app.MapPost("order/{Id:guid}/assign-rider", async (Guid Id, [FromBody] AssignRiderRequest body, ICommandHandler<AssignRiderCommand, OrderDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new AssignRiderCommand(Id, body.RiderId);

            Result<OrderDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<OrderDto>.Success(value, "Rider has been assigned to this order")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Order).RequireAuthorization();

        // Endpoint — add to your order group

        app.MapGet("order/{id:guid}/test-assign", async (
            Guid id,
            ICommandHandler<TestAutoAsyncCommand> handler,
            CancellationToken ct) =>
        {
            Result result = await handler.Handle(
                new TestAutoAsyncCommand(id), ct);

            return result.Match(
                () => Results.Ok(ApiResponse<string>.Success(
                    "Auto assignment triggered successfully.")),
                error => CustomResults.Problem(error));
        })
        .WithName("TestAutoAssign")
        .WithTags(Tags.Order)
        .RequireAuthorization()   // Admin only — test endpoint
        .Produces<ApiResponse<string>>()
        .Produces<ProblemDetails>(400);
    }
}
