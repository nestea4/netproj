using CatalogService.Models.DTOs;
using FluentValidation;

namespace CatalogService.Validators;

public class CreateMovieDetailsDtoValidator : AbstractValidator<CreateMovieDetailsDto>
{
    public CreateMovieDetailsDtoValidator()
    {
        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.Language)
            .MaximumLength(100).WithMessage("Language cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Language));

        RuleFor(x => x.AgeRating)
            .MaximumLength(20).WithMessage("Age rating cannot exceed 20 characters")
            .Must(BeValidAgeRating).WithMessage("Age rating must be one of: G, PG, PG-13, R, NC-17")
            .When(x => !string.IsNullOrEmpty(x.AgeRating));
    }

    private bool BeValidAgeRating(string? ageRating)
    {
        if (string.IsNullOrEmpty(ageRating))
            return true;

        var validRatings = new[] { "G", "PG", "PG-13", "R", "NC-17", "U", "12A", "15", "18" };
        return validRatings.Contains(ageRating.ToUpper());
    }
}
