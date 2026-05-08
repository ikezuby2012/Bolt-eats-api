using Application.Payment.Dto;
using Domain.Users;

namespace Application.Abstractions.Services.Payments;

public interface IPaymentGateway
{
    int Gateway { get; }
    Task<CreateIntentResult> CreateIntentAsync(
        CreateIntentRequest request,
        CancellationToken cancellationToken = default);

    Task<ConfirmPaymentResult> ConfirmPaymentAsync(
        string gatewayReference,
        CancellationToken cancellationToken = default);

    Task<RefundResult> RefundAsync(
        RefundRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SavedPaymentMethod>> ListPaymentMethodsAsync(
        string gatewayCustomerId,
        CancellationToken cancellationToken = default);

    Task<AttachMethodResult> AttachPaymentMethodAsync(
        string gatewayCustomerId,
        string paymentMethodToken,
        CancellationToken cancellationToken = default);

    Task DetachPaymentMethodAsync(
        string paymentMethodId,
        CancellationToken cancellationToken = default);
    Task<string> CreateCustomerAsync(User user, CancellationToken ct);
}
