using CatalogService.Models;

namespace CatalogService.Specifications.MovieSpecifications;

/// <summary>
/// Специфікація для популярних фільмів
/// </summary>
public class PopularMoviesSpecification : BaseSpecification<Movie>
{
    public PopularMoviesSpecification(int take = 10)
        : base(m => !m.IsDeleted && m.Rating >= 7.0m)
    {
        AddInclude(m => m.MovieCategories);
        AddInclude("MovieCategories.Category");
        
        ApplyOrderByDescending(m => m.Rating);
        ApplyPaging(0, take);
    }
}