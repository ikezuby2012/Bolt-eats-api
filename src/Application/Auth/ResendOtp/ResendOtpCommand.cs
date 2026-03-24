using Application.Abstractions.Messaging;

namespace Application.Auth.ResendOtp;

public sealed record ResendOtpCommand(string email) : ICommand<Guid>;
