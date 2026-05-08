namespace Application.Payment.Dto;

public sealed record WebhookParseResult
{
    public bool IsValid { get; init; }
    public WebhookEventType EventType { get; init; }
    public string? GatewayReference { get; init; }   // PaymentIntent ID
    public string? FailureCode { get; init; }
    public string? FailureMessage { get; init; }
    public string? RefundReference { get; init; }
    public decimal? RefundAmount { get; init; }
    public bool IsFullRefund { get; init; }
    public decimal? ReportedAmount { get; init; }

    public static WebhookParseResult Invalid(string? failureMessage = "") => new() { IsValid = false, FailureMessage = failureMessage };
}

public enum WebhookEventType
{
    Unknown,
    PaymentSucceeded,
    PaymentFailed,
    PaymentRefunded,
    PaymentDisputed,
}
