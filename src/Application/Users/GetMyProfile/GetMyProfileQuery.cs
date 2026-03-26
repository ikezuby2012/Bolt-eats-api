using Application.Abstractions.Messaging;
using Application.Users.Dto;

namespace Application.Users.GetMyProfile;

public sealed record GetMyProfileQuery() : IQuery<UserDto>;
