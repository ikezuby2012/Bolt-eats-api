using Application.Abstractions.Messaging;
using Application.Users.Dto;

namespace Application.Users.UpdateMyProfile;

public sealed record UpdateMyProfileCommand(string? firstName, string? lastName, string? phoneNumber, DateTime? dateOfBirth) :
    ICommand<UserDto>;
