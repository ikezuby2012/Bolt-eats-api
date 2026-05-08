using Application.Abstractions.Services.Payments;
using Application.Payment.Dto;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Infrastructure.Services.Payment;

internal sealed class StripeWebhookParser(IConfiguration config) : IWebhookParser
{
    public WebhookParseResult Parse(string rawBody, string signature)
    {
        try
        {
            Event stripeEvent = EventUtility.ConstructEvent(
                rawBody, signature, config["Stripe:WebhookSecret"]!);

            return stripeEvent.Type switch
            {
                "payment_intent.succeeded" => MapSucceeded(stripeEvent),
                "payment_intent.payment_failed" => MapFailed(stripeEvent),
                "charge.refunded" => MapRefunded(stripeEvent),
                _ => new WebhookParseResult { IsValid = true, EventType = WebhookEventType.Unknown }
            };
        }
        catch (StripeException ex)
        {
            return WebhookParseResult.Invalid(ex.Message);
        }
    }

    private static WebhookParseResult MapSucceeded(Event e)
    {
        if (e.Data.Object is not PaymentIntent intent)
        {
            return WebhookParseResult.Invalid("Not a vaild object");
        }


        return new()
        {
            IsValid = true,
            EventType = WebhookEventType.PaymentSucceeded,
            GatewayReference = intent.Id,
        };
    }

    private static WebhookParseResult MapFailed(Event e)
    {
        if (e.Data.Object is not PaymentIntent intent)
        {
            return WebhookParseResult.Invalid();
        }

        return new()
        {
            IsValid = true,
            EventType = WebhookEventType.PaymentFailed,
            GatewayReference = intent.Id,
            FailureCode = intent.LastPaymentError?.Code,
            FailureMessage = intent.LastPaymentError?.Message,
        };
    }

    private static WebhookParseResult MapRefunded(Event e)
    {
        if (e.Data.Object is not Charge charge)
        {
            return WebhookParseResult.Invalid();
        }

        Refund? refund = charge.Refunds.Data.FirstOrDefault();

        return new()
        {
            IsValid = true,
            EventType = WebhookEventType.PaymentRefunded,
            GatewayReference = charge.PaymentIntentId,
            RefundReference = refund?.Id,
            RefundAmount = refund is not null ? refund.Amount / 100m : null,
            IsFullRefund = charge.Refunded,
        };
    }
}
