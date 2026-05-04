using Application.Abstractions.Messaging;
using Application.Reviews.Dto;

namespace Application.Reviews.RemoveReview;

public sealed record RemoveReviewCommand(Guid ReviewId) : ICommand;
