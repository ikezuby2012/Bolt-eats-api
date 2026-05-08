using Application.Abstractions.Messaging;

namespace Application.Payment.HandleStripeWebhook;

public sealed record HandleStripeWebhookCommand(string RawBody, string StripeSignature) : ICommand;
