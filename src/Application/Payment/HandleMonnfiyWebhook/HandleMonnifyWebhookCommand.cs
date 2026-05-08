using Application.Abstractions.Messaging;

namespace Application.Payment.HandleMonnfiyWebhook;

public sealed record HandleMonnifyWebhookCommand(string RawBody, string MonnifySignature) : ICommand;
