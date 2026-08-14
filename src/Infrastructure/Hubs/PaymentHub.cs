using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs;

[Authorize]
public sealed class PaymentHub : Hub
{
    public async Task SubscribeToPayment(string paymentId) => await Groups.AddToGroupAsync(Context.ConnectionId, PaymentGroup(paymentId));

    public async Task UnsubscribeFromPayment(string paymentId) => await Groups.RemoveFromGroupAsync(Context.ConnectionId, PaymentGroup(paymentId));

    public static string PaymentGroup(string paymentId) => $"payment:{paymentId}";

    public static string PaymentGroup(Guid paymentId) => $"payment:{paymentId}";
}
