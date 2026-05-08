using Application.Payment.Dto;

namespace Application.Abstractions.Services.Payments;

public interface IWebhookParser
{
    WebhookParseResult Parse(string rawBody, string signature);
}
