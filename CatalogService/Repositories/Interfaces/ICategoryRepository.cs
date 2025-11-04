using CatalogService.Models;

namespace CatalogService.Repositories.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetCategoriesWithMoviesAsync(CancellationToken cancellationToken = default);
    Task<int> GetMovieCountByCategoryAsync(long categoryId, CancellationToken cancellationToken = default);
}