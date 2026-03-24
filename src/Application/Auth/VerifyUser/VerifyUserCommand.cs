using Application.Abstractions.Messaging;
using Application.Users.Dto;

namespace Application.Auth.VerifyUser;

public sealed record VerifyUserCommand(string email, string otp) : ICommand<CreatedUserDto>;
