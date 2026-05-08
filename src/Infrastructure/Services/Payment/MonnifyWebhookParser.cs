using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Services.Payments;
using Application.Payment.Dto;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Payment;

#pragma warning disable CA1308 // Normalize strings to uppercase
internal sealed class MonnifyWebhookParser(IConfiguration config) : IWebhookParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public WebhookParseResult Parse(string rawBody, string signature)
    {

        if (!VerifySignature(rawBody, signature))
        {
            return WebhookParseResult.Invalid();
        }

        MonnifyWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<MonnifyWebhookPayload>(rawBody, JsonOptions);
        }
        catch
        {
            return WebhookParseResult.Invalid();
        }

        if (payload?.EventData is null)
        {
            return WebhookParseResult.Invalid();
        }

        return payload.EventType switch
        {
            "SUCCESSFUL_TRANSACTION" => MapSucceeded(payload.EventData),
            "FAILED_TRANSACTION" => MapFailed(payload.EventData),
            "REVERSED_TRANSACTION" => MapReversed(payload.EventData),
            "DISPUTED_TRANSACTION" => MapDisputed(payload.EventData),
            _ => new WebhookParseResult
            {
                IsValid = true,
                EventType = WebhookEventType.Unknown
            }
        };
    }

    private static WebhookParseResult MapSucceeded(MonnifyEventData e) => new()
    {
        IsValid = true,
        EventType = WebhookEventType.PaymentSucceeded,
        GatewayReference = e.PaymentReference,
        ReportedAmount = e.AmountPaid,
    };

    private static WebhookParseResult MapFailed(MonnifyEventData e) => new()
    {
        IsValid = true,
        EventType = WebhookEventType.PaymentFailed,
        GatewayReference = e.PaymentReference,
        FailureCode = "monnify_failed",
        FailureMessage = $"Transaction failed. Status: {e.PaymentStatus}.",
    };

    private static WebhookParseResult MapReversed(MonnifyEventData e) => new()
    {
        IsValid = true,
        EventType = WebhookEventType.PaymentRefunded,
        GatewayReference = e.PaymentReference,
        RefundReference = e.TransactionReference,
        RefundAmount = e.AmountPaid,
        IsFullRefund = true,
    };

    private static WebhookParseResult MapDisputed(MonnifyEventData e) => new()
    {
        IsValid = true,
        EventType = WebhookEventType.PaymentDisputed,
        GatewayReference = e.PaymentReference,
        FailureCode = "disputed",
        FailureMessage = $"Chargeback raised. Transaction: {e.TransactionReference}.",
    };

    private bool VerifySignature(string rawBody, string receivedSignature)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(config["Monnify:SecretKey"]!);
        byte[] bodyBytes = Encoding.UTF8.GetBytes(rawBody);

        using var hmac = new HMACSHA512(keyBytes);
        string computedHex = Convert.ToHexString(hmac.ComputeHash(bodyBytes))
                                   .ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHex),
            Encoding.UTF8.GetBytes(receivedSignature.ToLowerInvariant()));
    }
}
