using CatalogService.Models;

namespace CatalogService.Specifications.ShowtimeSpecifications;

/// <summary>
/// Специфікація для фільтрації сеансів
/// </summary>
public class ShowtimeFilterSpecification : BaseSpecification<Showtime>
{
    public ShowtimeFilterSpecification(
        long? movieId = null,
        long? hallId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        bool? isActive = null,
        int? pageNumber = null,
        int? pageSize = null)
        : base(BuildCriteria(movieId, hallId, startDate, endDate, isActive))
    {
        AddInclude(s => s.Movie);
        AddInclude(s => s.Hall);
        
        ApplyOrderBy(s => s.StartTime);

        if (pageNumber.HasValue && pageSize.HasValue)
        {
            ApplyPaging((pageNumber.Value - 1) * pageSize.Value, pageSize.Value);
        }
    }

    private static System.Linq.Expressions.Expression<Func<Showtime, bool>> BuildCriteria(
        long? movieId,
        long? hallId,
        DateTime? startDate,
        DateTime? endDate,
        bool? isActive)
    {
        return s =>
            !s.IsDeleted &&
            (!movieId.HasValue || s.MovieId == movieId.Value) &&
            (!hallId.HasValue || s.HallId == hallId.Value) &&
            (!startDate.HasValue || s.StartTime >= startDate.Value) &&
            (!endDate.HasValue || s.StartTime <= endDate.Value) &&
            (!isActive.HasValue || s.IsActive == isActive.Value);
    }
}