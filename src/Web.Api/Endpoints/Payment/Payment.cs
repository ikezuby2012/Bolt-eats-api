using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Payment.AttachPayment;
using Application.Payment.ConfirmPayment;
using Application.Payment.CreatePaymentIntent;
using Application.Payment.DetachPaymentMethod;
using Application.Payment.Dto;
using Application.Payment.GetPaymentHistory;
using Application.Payment.GetPaymentMethods;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Payment;

public class Payment : IEndpoint
{
    internal sealed record CreateIntentRequest(Guid CartId, int GatewayId = 2); // lets use monnify as our default for now
    internal sealed record ConfirmPaymentRequest(Guid PaymentId, string? CustomerNotes);
    internal sealed record AttachMethodRequest(string PaymentMethodToken, int GatewayId = 2);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("payments").WithTags(Tags.Payment);

        group.MapPost("/intent", async ([FromBody] CreateIntentRequest body, ICommandHandler<CreatePaymentIntentCommand, PaymentIntentDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new CreatePaymentIntentCommand(body.CartId, body.GatewayId);

            Result<PaymentIntentDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaymentIntentDto>.Success(value, "Payment Intent Created Successfully")), error => CustomResults.Problem(error));
        }).WithName("CreatePaymentIntent")
        .RequireAuthorization()
        .Produces<PaymentIntentDto>()
        .Produces(402)
        .Produces<ProblemDetails>(400);

        group.MapPost("/confirm", async ([FromBody] ConfirmPaymentRequest body, ICommandHandler<ConfirmPaymentCommand, PaymentConfirmationDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new ConfirmPaymentCommand(body.PaymentId, body.CustomerNotes);

            Result<PaymentConfirmationDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaymentConfirmationDto>.Success(value, "Payment Confirmed Successfully")), error => CustomResults.Problem(error));
        }).WithName("ConfirmPayment")
        .RequireAuthorization()
        .Produces<PaymentConfirmationDto>()
        .Produces(402)
        .Produces<ProblemDetails>(400);

        group.MapGet("/methods", async (IQueryHandler<GetPaymentMethodQuery, IReadOnlyList<PaymentMethodDto>> handler, IUserContext userContext, CancellationToken cancellationToken, [FromQuery] int GatewayId = 2) =>
        {
            Guid userId = userContext.UserId;

            var query = new GetPaymentMethodQuery(userId, GatewayId);
            Result<IReadOnlyList<PaymentMethodDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<IReadOnlyList<PaymentMethodDto>>.Success(value, "All Payment Methods")), error => CustomResults.Problem(error));

        }).WithName("GetPaymentMethods")
        .RequireAuthorization()
        .Produces<IReadOnlyList<PaymentMethodDto>>();

        group.MapPost("/methods", async ([FromBody] AttachMethodRequest body, ICommandHandler<AttachPaymentMethodCommand, PaymentMethodDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new AttachPaymentMethodCommand(body.PaymentMethodToken, body.GatewayId);
            Result<PaymentMethodDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaymentMethodDto>.Success(value, "Created a Payment method Successfully")), error => CustomResults.Problem(error));
        }).WithName("AttachPaymentMethod")
        .RequireAuthorization()
        .Produces<PaymentMethodDto>()
        .Produces<ProblemDetails>(400);

        group.MapDelete("/methods/{id}", async (
            string id,
            ICommandHandler<DetachPaymentMethodCommand> handler,
            CancellationToken cancellationToken,
            [FromQuery] int GatewayId = 2) =>
        {
            var command = new DetachPaymentMethodCommand(id, GatewayId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                 () => Results.NoContent(),
                 error => CustomResults.Problem(error));
        })
        .WithName("DetachPaymentMethod")
        .RequireAuthorization()
        .Produces(204)
        .Produces<ProblemDetails>(400);

        group.MapGet("/history", async (
            IQueryHandler<GetPaymentHistoryQuery, PaginatedResult<PaymentHistoryDto>> handler,
            IUserContext context,
            CancellationToken cancellationToken,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var query = new GetPaymentHistoryQuery(context.UserId, page, pageSize);

            Result<PaginatedResult<PaymentHistoryDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaginatedResult<PaymentHistoryDto>>.Success(value, "All Payments History")), error => CustomResults.Problem(error));
        })
        .WithName("GetPaymentHistory")
        .RequireAuthorization()
        .Produces<PaginatedResult<PaymentHistoryDto>>();
    }
}
