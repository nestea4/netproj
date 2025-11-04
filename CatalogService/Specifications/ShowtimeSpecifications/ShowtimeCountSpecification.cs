using CatalogService.Models;

namespace CatalogService.Specifications.ShowtimeSpecifications;

/// <summary>
/// Специфікація для підрахунку сеансів (без пагінації)
/// </summary>
public class ShowtimeCountSpecification : BaseSpecification<Showtime>
{
    public ShowtimeCountSpecification(ISpecification<Showtime> originalSpec)
        : base(originalSpec.Criteria)
    {
        //копіюю тільки критерії фільтрації, без пагінації
    }
}