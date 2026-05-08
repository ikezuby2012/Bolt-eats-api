using SharedKernel;

namespace Domain.Payment;

public sealed record PaymentConfirmedEvent(Guid PaymentId, string? CustomerNotes) : IDomainEvent;
