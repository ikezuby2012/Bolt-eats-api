using Application.Abstractions.Messaging;
using Application.Users.Dto;

namespace Application.Users.ReviewRiderProfileCommand;

public sealed record ReviewRiderProfileCommand(
    Guid RiderProfileId,
    bool Approved,
    string? RejectionReason,
    Guid ReviewedBy
) : ICommand<RiderProfileDto>;
