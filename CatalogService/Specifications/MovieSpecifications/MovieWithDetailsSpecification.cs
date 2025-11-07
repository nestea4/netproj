using CatalogService.Models;

namespace CatalogService.Specifications.MovieSpecifications;

/// <summary>
/// Специфікація для завантаження фільму з усіма деталями
/// </summary>
public class MovieWithDetailsSpecification : BaseSpecification<Movie>
{
    public MovieWithDetailsSpecification(long movieId)
        : base(m => m.MovieId == movieId && !m.IsDeleted)
    {
        AddInclude(m => m.Details);
        AddInclude(m => m.MovieCategories);
        AddInclude("MovieCategories.Category");
        AddInclude(m => m.Showtimes);
        AddInclude("Showtimes.Hall");
    }
}