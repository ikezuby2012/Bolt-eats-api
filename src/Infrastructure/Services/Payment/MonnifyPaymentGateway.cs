using System.Net.Http.Json;
using Application.Abstractions.Services;
using Application.Abstractions.Services.Payments;
using Application.Payment.Dto;
using Domain.Payment;
using Domain.Users;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Payment;

public sealed class MonnifyPaymentGateway(IConfiguration config, HttpClient http, ITokenCache cache) : IPaymentGateway
{
    public int Gateway => Domain.Payment.PaymentGateway.Monnify.Id;

    private string BaseUrl => config["Monnify:BaseUrl"]!;
    private string ApiKey => config["Monnify:ApiKey"]!;
    private string SecretKey => config["Monnify:SecretKey"]!;
    private string ContractCode => config["Monnify:ContractCode"]!;

    private const string TOKEN_KEY = "monnify:access_token";

    public Task<AttachMethodResult> AttachPaymentMethodAsync(string gatewayCustomerId, string paymentMethodToken, CancellationToken cancellationToken = default)
           => Task.FromResult(new AttachMethodResult(false, "Monnify does not support saved payment methods."));

    public async Task<ConfirmPaymentResult> ConfirmPaymentAsync(string gatewayReference, CancellationToken cancellationToken = default)
    {
        string token = await AuthenticateAsync(cancellationToken);

        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        string encoded = Uri.EscapeDataString(gatewayReference);
        MonnifyStatusResponse? response = await http.GetFromJsonAsync<MonnifyStatusResponse>(
            $"{BaseUrl}/api/v2/transactions/{encoded}",
            cancellationToken);

        return response?.ResponseBody?.PaymentStatus switch
        {
            "PAID" => new ConfirmPaymentResult(true, PaymentStatus.Succeeded),
            "FAILED" => new ConfirmPaymentResult(false, PaymentStatus.Failed,
                               "monnify_failed", "Payment failed at gateway."),
            "OVERPAID" => new ConfirmPaymentResult(true, PaymentStatus.Succeeded),
            "ABANDONED" => new ConfirmPaymentResult(false, PaymentStatus.Failed,
                               "monnify_abandoned", "Payment was abandoned."),
            _ => new ConfirmPaymentResult(false, PaymentStatus.Processing,
                     "processing", "Payment still processing.")
        };
    }

    public async Task<CreateIntentResult> CreateIntentAsync(CreateIntentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            string token = await AuthenticateAsync(cancellationToken);
            string reference = $"UE-{request.OrderId}-{Guid.NewGuid():N}"[..30];

            var payload = new
            {
                amount = request.Amount,
                customerName = "Customer",
                customerEmail = "customer@example.com",
                paymentReference = reference,
                paymentDescription = request.Description ?? "Chill Eats",
                currencyCode = request.Currency.ToUpperInvariant(),
                contractCode = ContractCode,
                paymentMethods = new[] { "CARD", "ACCOUNT_TRANSFER" }
            };

            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await http.PostAsJsonAsync(
                $"{BaseUrl}/api/v1/merchant/transactions/init-transaction",
                payload, cancellationToken);

            MonnifyInitResponse? result = await response.Content
                .ReadFromJsonAsync<MonnifyInitResponse>(cancellationToken: cancellationToken);

            if (result?.RequestSuccessful != true)
            {

                return new CreateIntentResult(false, null, null,
                    "monnify_error", result?.ResponseMessage ?? "Unknown error");
            }


            return new CreateIntentResult(
                IsSuccess: true,
                GatewayReference: reference,
                ClientSecret: result.ResponseBody?.CheckoutUrl);

        }
        catch (Exception ex)
        {
            return new CreateIntentResult(false, null, null,
                "monnify_exception", ex.Message);
        }
    }

    public Task DetachPaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<SavedPaymentMethod>> ListPaymentMethodsAsync(string gatewayCustomerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<SavedPaymentMethod>>([]);
    }

    public async Task<RefundResult> RefundAsync(RefundRequest request, CancellationToken cancellationToken = default)
    {
        string token = await AuthenticateAsync(cancellationToken);

        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        string refundRef = $"REF-{Guid.NewGuid():N}"[..20];
        var payload = new
        {
            transactionReference = request.GatewayReference,
            refundReason = request.Reason,
            customerNote = request.Reason,
            refundAmount = request.Amount,
            refundReference = refundRef
        };

        HttpResponseMessage response = await http.PostAsJsonAsync(
            $"{BaseUrl}/api/v1/refunds/initiate-refund",
            payload, cancellationToken);

        MonnifyRefundResponse? result = await response.Content
            .ReadFromJsonAsync<MonnifyRefundResponse>(cancellationToken: cancellationToken);

        return result?.RequestSuccessful == true
            ? new RefundResult(true, refundRef)
            : new RefundResult(false, null, result?.ResponseMessage ?? "Refund failed.");
    }

    private async Task<string> AuthenticateAsync(CancellationToken cancellationToken)
    {
        string? cached = await cache.GetAsync(TOKEN_KEY);

        if (!string.IsNullOrEmpty(cached))
        {
            return cached;
        }
        string credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{ApiKey}:{SecretKey}"));

        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        HttpResponseMessage response = await http.PostAsync($"{BaseUrl}/api/v1/auth/login", null, cancellationToken);

        MonnifyAuthResponse? result = await response.Content
            .ReadFromJsonAsync<MonnifyAuthResponse>(cancellationToken: cancellationToken);

        await cache.SetAsync(TOKEN_KEY, result!.ResponseBody!.AccessToken, TimeSpan.FromSeconds(result.ResponseBody.ExpiresIn - 60));

        return result!.ResponseBody!.AccessToken;
    }

    public Task<string> CreateCustomerAsync(User user, CancellationToken ct) => Task.FromResult("Not really needed");


    // ── Monnify response shapes ───────────────────────────────────────────────
    private sealed record MonnifyAuthResponse(bool RequestSuccessful, MonnifyAuthBody? ResponseBody, string? ResponseMessage);
    private sealed record MonnifyAuthBody(string AccessToken, int ExpiresIn);
    private sealed record MonnifyInitResponse(bool RequestSuccessful, MonnifyInitBody? ResponseBody, string? ResponseMessage);
    private sealed record MonnifyInitBody(string CheckoutUrl, string TransactionReference);
    private sealed record MonnifyStatusResponse(bool RequestSuccessful, MonnifyStatusBody? ResponseBody);
    private sealed record MonnifyStatusBody(string PaymentStatus, decimal Amount);
    private sealed record MonnifyRefundResponse(bool RequestSuccessful, string? ResponseMessage);
}
