using Application.Abstractions.Messaging;
using Application.Auth.Dto;

namespace Application.Auth.VerifyUser;

public sealed record VerifyUserCommand(string email, string otp) : ICommand<LoginSuccessDto>;
