using System.Threading;
using Application.Abstractions.Messaging;
using Application.Payment.HandleMonnfiyWebhook;
using Application.Payment.HandleStripeWebhook;

namespace Web.Api.Endpoints.Payment;

public class Webhooks : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("payments/webhook").WithTags(Tags.Payment);

        group.MapPost("/stripe", async (HttpContext ctx, ICommandHandler<HandleStripeWebhookCommand> handler, CancellationToken cancellationToken) =>
        {
            // Must read raw body before any middleware touches it
            using var reader = new StreamReader(ctx.Request.Body);
            string rawBody = await reader.ReadToEndAsync(cancellationToken);
            string signature = ctx.Request.Headers["Stripe-Signature"].FirstOrDefault() ?? string.Empty;

            var command = new HandleStripeWebhookCommand(rawBody, signature);
            await handler.Handle(command, cancellationToken);
            return Results.Ok();
        })
        .WithName("StripeWebhook")
        .Produces(200);

        group.MapPost("/monnify", async (HttpContext ctx, ICommandHandler<HandleMonnifyWebhookCommand> handler, CancellationToken cancellationToken) =>
        {
            using var reader = new StreamReader(ctx.Request.Body);
            string rawBody = await reader.ReadToEndAsync(cancellationToken);
            string signature = ctx.Request.Headers["monnify-signature"].FirstOrDefault() ?? string.Empty;

            var command = new HandleMonnifyWebhookCommand(rawBody, signature);
            await handler.Handle(command, cancellationToken);
            return Results.Ok();
        }).WithName("MonnifyWebhook").Produces(200);
    }
}
