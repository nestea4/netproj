using CatalogService.Models.DTOs;
using FluentValidation;

namespace CatalogService.Validators;

public class MovieFilterValidator : AbstractValidator<MovieFilterParameters>
{
    public MovieFilterValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");

        RuleFor(x => x.MinRating)
            .InclusiveBetween(0, 10).WithMessage("Minimum rating must be between 0 and 10")
            .When(x => x.MinRating.HasValue);

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField).WithMessage("Invalid sort field. Valid values: title, rating, releasedate, duration")
            .When(x => !string.IsNullOrEmpty(x.SortBy));
    }

    private bool BeValidSortField(string? sortBy)
    {
        if (string.IsNullOrEmpty(sortBy))
            return true;

        var validFields = new[] { "title", "rating", "releasedate", "duration" };
        return validFields.Contains(sortBy.ToLower());
    }
}
