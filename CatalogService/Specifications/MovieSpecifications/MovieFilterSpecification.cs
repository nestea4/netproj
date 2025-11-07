using CatalogService.Models;

namespace CatalogService.Specifications.MovieSpecifications;

/// <summary>
/// Специфікація для фільтрації фільмів
/// </summary>
public class MovieFilterSpecification : BaseSpecification<Movie>
{
    public MovieFilterSpecification(
        string? searchTerm = null,
        long? categoryId = null,
        decimal? minRating = null,
        DateTime? releaseDateFrom = null,
        DateTime? releaseDateTo = null,
        string? sortBy = null,
        bool sortDescending = false,
        int? pageNumber = null,
        int? pageSize = null)
        : base(BuildCriteria(searchTerm, categoryId, minRating, releaseDateFrom, releaseDateTo))
    {
        //Includes
        AddInclude(m => m.MovieCategories);
        AddInclude("MovieCategories.Category");
        AddInclude(m => m.Details);

        //сортування
        ApplySorting(sortBy, sortDescending);

        //пагінація
        if (pageNumber.HasValue && pageSize.HasValue)
        {
            ApplyPaging((pageNumber.Value - 1) * pageSize.Value, pageSize.Value);
        }
    }

    private static System.Linq.Expressions.Expression<Func<Movie, bool>> BuildCriteria(
        string? searchTerm,
        long? categoryId,
        decimal? minRating,
        DateTime? releaseDateFrom,
        DateTime? releaseDateTo)
    {
        return m =>
            !m.IsDeleted &&
            (searchTerm == null ||
             m.Title.ToLower().Contains(searchTerm.ToLower()) ||
             m.Director.ToLower().Contains(searchTerm.ToLower())) &&
            (!categoryId.HasValue ||
             m.MovieCategories.Any(mc => mc.CategoryId == categoryId.Value)) &&
            (!minRating.HasValue || m.Rating >= minRating.Value) &&
            (!releaseDateFrom.HasValue || m.ReleaseDate >= releaseDateFrom.Value) &&
            (!releaseDateTo.HasValue || m.ReleaseDate <= releaseDateTo.Value);
    }

    private void ApplySorting(string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            ApplyOrderByDescending(m => m.ReleaseDate);
            return;
        }

        var sortByLower = sortBy.ToLower();

        switch (sortByLower)
        {
            case "title":
                if (sortDescending)
                    ApplyOrderByDescending(m => m.Title);
                else
                    ApplyOrderBy(m => m.Title);
                break;

            case "rating":
                if (sortDescending)
                    ApplyOrderByDescending(m => m.Rating);
                else
                    ApplyOrderBy(m => m.Rating);
                break;

            case "releasedate":
                if (sortDescending)
                    ApplyOrderByDescending(m => m.ReleaseDate);
                else
                    ApplyOrderBy(m => m.ReleaseDate);
                break;

            case "duration":
                if (sortDescending)
                    ApplyOrderByDescending(m => m.DurationMinutes);
                else
                    ApplyOrderBy(m => m.DurationMinutes);
                break;

            default:
                ApplyOrderByDescending(m => m.ReleaseDate);
                break;
        }
    }
}