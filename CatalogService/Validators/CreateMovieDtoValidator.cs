using CatalogService.Models.DTOs;
using FluentValidation;

namespace CatalogService.Validators;

public class CreateMovieDtoValidator : AbstractValidator<CreateMovieDto>
{
    public CreateMovieDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255).WithMessage("Title cannot exceed 255 characters");

        RuleFor(x => x.OriginalTitle)
            .MaximumLength(255).WithMessage("Original title cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.OriginalTitle));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0")
            .LessThanOrEqualTo(600).WithMessage("Duration cannot exceed 600 minutes");

        RuleFor(x => x.ReleaseDate)
            .NotEmpty().WithMessage("Release date is required")
            .LessThanOrEqualTo(DateTime.Now.AddYears(5))
                .WithMessage("Release date cannot be more than 5 years in the future");

        RuleFor(x => x.Director)
            .MaximumLength(200).WithMessage("Director name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Director));

        RuleFor(x => x.Rating)
            .InclusiveBetween(0, 10).WithMessage("Rating must be between 0 and 10")
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.PosterUrl)
            .Must(BeAValidUrl).WithMessage("Poster URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.PosterUrl));

        RuleFor(x => x.TrailerUrl)
            .Must(BeAValidUrl).WithMessage("Trailer URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.TrailerUrl));

        RuleFor(x => x.CategoryIds)
            .NotEmpty().WithMessage("At least one category must be selected")
            .Must(x => x.All(id => id > 0)).WithMessage("All category IDs must be positive");

        RuleFor(x => x.Details)
            .SetValidator(new CreateMovieDetailsDtoValidator()!)
            .When(x => x.Details != null);
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;
            
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}