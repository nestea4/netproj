using FluentValidation;
using ReviewService.Application.Commands.Discussions;

namespace ReviewService.Application.Validators;

public class CreateDiscussionCommandValidator : AbstractValidator<CreateDiscussionCommand>
{
    public CreateDiscussionCommandValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0).WithMessage("Movie ID must be positive");

        RuleFor(x => x.Topic)
            .NotEmpty().WithMessage("Topic is required")
            .MaximumLength(200).WithMessage("Topic cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
    }
}