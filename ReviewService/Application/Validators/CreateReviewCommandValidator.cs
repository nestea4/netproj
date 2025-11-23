using FluentValidation;
using ReviewService.Application.Commands.Reviews;

namespace ReviewService.Application.Validators;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0).WithMessage("Movie ID must be positive");

        RuleFor(x => x.MovieTitle)
            .NotEmpty().WithMessage("Movie title is required")
            .MaximumLength(200).WithMessage("Movie title cannot exceed 200 characters");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be positive");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("User name is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Review title is required")
            .MaximumLength(200).WithMessage("Review title cannot exceed 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Review content is required")
            .MinimumLength(50).WithMessage("Review content must be at least 50 characters")
            .MaximumLength(5000).WithMessage("Review content cannot exceed 5000 characters");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 10).WithMessage("Rating must be between 1 and 10");
    }
}