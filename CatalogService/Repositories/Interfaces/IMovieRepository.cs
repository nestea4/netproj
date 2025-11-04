using CatalogService.Models;
using CatalogService.Specifications;

namespace CatalogService.Repositories.Interfaces;

public interface IMovieRepository : IGenericRepository<Movie>
{
    // Eager Loading - завантаження всіх зв'язаних даних одним запитом
    Task<Movie?> GetMovieWithDetailsAsync(long movieId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> GetMoviesWithCategoriesAsync(CancellationToken cancellationToken = default);
    
    // Explicit Loading - відкладене завантаження зв'язаних даних
    Task LoadMovieDetailsAsync(Movie movie, CancellationToken cancellationToken = default);
    Task LoadMovieCategoriesAsync(Movie movie, CancellationToken cancellationToken = default);
    
    // LINQ to Entities - складні запити
    Task<IEnumerable<Movie>> GetMoviesByCategoryAsync(long categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> SearchMoviesAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    // Specification Pattern
    Task<IEnumerable<Movie>> GetMoviesBySpecificationAsync(ISpecification<Movie> specification, CancellationToken cancellationToken = default);
    Task<int> CountBySpecificationAsync(ISpecification<Movie> specification, CancellationToken cancellationToken = default);
}