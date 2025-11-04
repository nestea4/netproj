using CatalogService.Models.DTOs;
using FluentValidation;

namespace CatalogService.Validators;

public class CreateShowtimeDtoValidator : AbstractValidator<CreateShowtimeDto>
{
    public CreateShowtimeDtoValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0).WithMessage("Movie ID must be valid");

        RuleFor(x => x.HallId)
            .GreaterThan(0).WithMessage("Hall ID must be valid");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required")
            .GreaterThanOrEqualTo(DateTime.Now).WithMessage("Start time cannot be in the past");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Base price must be non-negative")
            .LessThan(10000).WithMessage("Base price seems unreasonably high");
    }
}