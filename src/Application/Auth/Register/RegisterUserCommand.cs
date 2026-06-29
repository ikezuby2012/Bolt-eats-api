using Application.Abstractions.Messaging;
using Application.Users.Dto;

namespace Application.Auth.Register;

public sealed record RegisterUserCommand(string Email, string FirstName, string LastName, string phone, string Password) : ICommand<CreatedUserDto>;
