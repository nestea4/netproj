using FluentValidation;
using ReviewService.Application.Commands.Reviews;

namespace ReviewService.Application.Validators;

public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("Review ID is required");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be positive");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("User name is required");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required")
            .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters");
    }
}