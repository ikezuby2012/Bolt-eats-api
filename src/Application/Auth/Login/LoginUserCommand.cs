using Application.Abstractions.Messaging;
using Application.Auth.Dto;

namespace Application.Auth.Login;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<LoginSuccessDto>;
