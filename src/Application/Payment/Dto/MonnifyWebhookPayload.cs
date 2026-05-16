namespace Application.Payment.Dto;

public sealed class MonnifyWebhookPayload
{
    public string EventType { get; init; } = default!;
    public MonnifyEventData EventData { get; init; } = default!;
}

public sealed class MonnifyEventData
{
    // ── Core references ───────────────────────────────────────────────────
    public string PaymentReference { get; init; } = default!;  // your merchant ref
    public string TransactionReference { get; init; } = default!;  // MNFY|xx|... Monnify ref

    // ── Amounts (in Naira, NOT kobo — unlike Stripe) ──────────────────────
    public decimal AmountPaid { get; init; }
    public decimal TotalPayable { get; init; }

    // ── Status & metadata ─────────────────────────────────────────────────
    public string PaymentStatus { get; init; } = default!;   // "PAID", "FAILED" etc.
    public string PaymentMethod { get; init; } = default!;   // "CARD", "ACCOUNT_TRANSFER"
    public string Currency { get; init; } = default!;
    public string? PaidOn { get; init; }
    public string? PaymentDescription { get; init; }

    // ── Customer ──────────────────────────────────────────────────────────
    public MonnifyCustomer? Customer { get; init; }
}

public sealed class MonnifyCustomer
{
    public string? Name { get; init; }
    public string? Email { get; init; }
}
