using Application.Abstractions.Messaging;
using Application.Reviews.Dto;

namespace Application.Reviews.SubmitReview;

public sealed record SubmitReviewCommand(
    Guid OrderId,
    int Rating,
    string Comment) : ICommand<ReviewDto>;
