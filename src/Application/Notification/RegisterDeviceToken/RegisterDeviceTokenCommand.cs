using Application.Abstractions.Messaging;

namespace Application.Notification.RegisterDeviceToken;

public sealed record RegisterDeviceTokenCommand(string Token, string Platform) : ICommand;
