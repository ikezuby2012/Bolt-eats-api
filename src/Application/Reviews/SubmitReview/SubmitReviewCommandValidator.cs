using FluentValidation;

namespace Application.Reviews.SubmitReview;

public class SubmitReviewCommandValidator : AbstractValidator<SubmitReviewCommand>
{
    public SubmitReviewCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .NotEmpty()
            .WithMessage("A comment is required.")
            .MinimumLength(10)
            .WithMessage("Comment must be at least 10 characters.")
            .MaximumLength(1000)
            .WithMessage("Comment cannot exceed 1000 characters.");
    }
}
