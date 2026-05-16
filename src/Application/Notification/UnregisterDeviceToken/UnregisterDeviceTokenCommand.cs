using Application.Abstractions.Messaging;

namespace Application.Notification.UnregisterDeviceToken;

public sealed record UnregisterDeviceTokenCommand(string Token) : ICommand;
