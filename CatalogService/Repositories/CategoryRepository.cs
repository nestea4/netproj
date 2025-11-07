using CatalogService.Data;
using CatalogService.Models;
using CatalogService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(CatalogDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetCategoriesWithMoviesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.MovieCategories)
            .ThenInclude(mc => mc.Movie)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetMovieCountByCategoryAsync(long categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.MovieCategories
            .CountAsync(mc => mc.CategoryId == categoryId && !mc.Movie.IsDeleted, cancellationToken);
    }
}