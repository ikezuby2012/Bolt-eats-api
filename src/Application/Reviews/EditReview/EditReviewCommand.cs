using Application.Abstractions.Messaging;
using Application.Reviews.Dto;

namespace Application.Reviews.EditReview;

public record EditReviewCommand(
    Guid ReviewId,
    int Rating,
    string Comment) : ICommand<ReviewDto>;
