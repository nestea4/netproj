using CatalogService.Models.DTOs;
using FluentValidation;

namespace CatalogService.Validators;

public class UpdateMovieDtoValidator : AbstractValidator<UpdateMovieDto>
{
    public UpdateMovieDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255).WithMessage("Title cannot exceed 255 characters");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0")
            .LessThanOrEqualTo(600).WithMessage("Duration cannot exceed 600 minutes");

        RuleFor(x => x.ReleaseDate)
            .NotEmpty().WithMessage("Release date is required");

        RuleFor(x => x.Rating)
            .InclusiveBetween(0, 10).WithMessage("Rating must be between 0 and 10");
    }
}