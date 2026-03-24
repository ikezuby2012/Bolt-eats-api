using Application.Abstractions.Messaging;
using Application.Auth.Dto;

namespace Application.Auth.RefreshTokenAsync;

public sealed record RefreshTokenCommand(string accessToken, string refreshToken) : ICommand<LoginSuccessDto>;
