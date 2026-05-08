using Application.Abstractions.Services.Payments;
using Application.Payment.Dto;
using Domain.Payment;
using Domain.Users;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Infrastructure.Services.Payment;

#pragma warning disable CA1308 // Normalize strings to uppercase
internal sealed class StripePaymentGateway(IConfiguration config) : IPaymentGateway
{
    public int Gateway => PaymentGateway.Stripe.Id;

    private readonly PaymentIntentService _intentService = new();
    private readonly CustomerService _customerService = new();
    private readonly PaymentMethodService _methodService = new();
    private readonly RefundService _refundService = new();

    public async Task<AttachMethodResult> AttachPaymentMethodAsync(string gatewayCustomerId, string paymentMethodToken, CancellationToken cancellationToken = default)
    {
        try
        {
            await _methodService.AttachAsync(
                paymentMethodToken,
                new PaymentMethodAttachOptions { Customer = gatewayCustomerId },
                cancellationToken: cancellationToken);

            return new AttachMethodResult(true);
        }
        catch (StripeException ex)
        {
            return new AttachMethodResult(false, ex.StripeError.Message);
        }
    }

    public async Task<ConfirmPaymentResult> ConfirmPaymentAsync(string gatewayReference, CancellationToken cancellationToken = default)
    {
        try
        {
            PaymentIntent intent = await _intentService.GetAsync(gatewayReference,
                cancellationToken: cancellationToken);

            return intent.Status switch
            {
                "succeeded" => new ConfirmPaymentResult(true, PaymentStatus.Succeeded),
                "requires_payment_method" or "canceled" => new ConfirmPaymentResult(
                    false, PaymentStatus.Failed,
                    intent.LastPaymentError?.Code,
                    intent.LastPaymentError?.Message),
                _ => new ConfirmPaymentResult(
                    false, PaymentStatus.Processing,
                    "processing",
                    "Payment is still processing.")
            };
        }
        catch (StripeException ex)
        {
            return new ConfirmPaymentResult(
                false, PaymentStatus.Failed,
                ex.StripeError.Code,
                ex.StripeError.Message);
        }
    }

    public async Task<CreateIntentResult> CreateIntentAsync(CreateIntentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            StripeConfiguration.ApiKey = config["Stripe:SecretKey"];

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100),  // kobo
                Currency = request.Currency.ToLowerInvariant(),
                Customer = request.GatewayCustomerId,
                Description = request.Description,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Metadata = request.Metadata?.ToDictionary(k => k.Key, v => v.Value)
            };

            PaymentIntent intent = await _intentService.CreateAsync(options,
                cancellationToken: cancellationToken);

            return new CreateIntentResult(
                IsSuccess: true,
                GatewayReference: intent.Id,
                ClientSecret: intent.ClientSecret);
        }
        catch (StripeException ex)
        {
            return new CreateIntentResult(
                IsSuccess: false,
                GatewayReference: null,
                ClientSecret: null,
                FailureCode: ex.StripeError.Code,
                FailureMessage: ex.StripeError.Message);
        }
    }

    public async Task DetachPaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken = default)
    {
        await _methodService.DetachAsync(paymentMethodId,
             cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<SavedPaymentMethod>> ListPaymentMethodsAsync(string gatewayCustomerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(gatewayCustomerId))
        { return []; }


        var options = new PaymentMethodListOptions
        {
            Customer = gatewayCustomerId,
            Type = "card"
        };

        StripeList<PaymentMethod> methods = await _methodService.ListAsync(options,
            cancellationToken: cancellationToken);

        Customer customer = await _customerService.GetAsync(gatewayCustomerId,
            cancellationToken: cancellationToken);

        string? defaultMethodId = customer.InvoiceSettings?.DefaultPaymentMethodId;

        return methods.Data.Select(m => new SavedPaymentMethod(
            m.Id,
            m.Card.Brand,
            m.Card.Last4,
            (int)m.Card.ExpMonth,
            (int)m.Card.ExpYear,
            m.Id == defaultMethodId)).ToList();
    }

    public async Task<RefundResult> RefundAsync(RefundRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            Refund refund = await _refundService.CreateAsync(new RefundCreateOptions
            {
                PaymentIntent = request.GatewayReference,
                Amount = (long)(request.Amount * 100),
                Reason = RefundReasons.RequestedByCustomer
            }, cancellationToken: cancellationToken);

            return new RefundResult(true, refund.Id);
        }
        catch (StripeException ex)
        {
            return new RefundResult(false, null, ex.StripeError.Message);
        }
    }

    public async Task<string> CreateCustomerAsync(User user, CancellationToken ct)
    {
        Customer customer = await _customerService.CreateAsync(new CustomerCreateOptions
        {
            Email = user.Email,
            Name = $"{user.FirstName} {user.LastName}"
        }, cancellationToken: ct);

        return customer.Id;
    }
}
