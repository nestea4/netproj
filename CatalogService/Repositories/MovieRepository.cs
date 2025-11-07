using CatalogService.Data;
using CatalogService.Models;
using CatalogService.Repositories.Interfaces;
using CatalogService.Specifications;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repositories;

public class MovieRepository : GenericRepository<Movie>, IMovieRepository
{
    public MovieRepository(CatalogDbContext context) : base(context)
    {
    }

    // EAGER LOADING - завантаження всіх даних одним запитом
    public async Task<Movie?> GetMovieWithDetailsAsync(long movieId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.Details)
            .Include(m => m.MovieCategories)
                .ThenInclude(mc => mc.Category)
            .Include(m => m.Showtimes.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Hall)
            .FirstOrDefaultAsync(m => m.MovieId == movieId && !m.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Movie>> GetMoviesWithCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.MovieCategories)
                .ThenInclude(mc => mc.Category)
            .Where(m => !m.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    // EXPLICIT LOADING - завантаження зв'язаних даних за потребою
    public async Task LoadMovieDetailsAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        await _context.Entry(movie)
            .Reference(m => m.Details)
            .LoadAsync(cancellationToken);
    }

    public async Task LoadMovieCategoriesAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        await _context.Entry(movie)
            .Collection(m => m.MovieCategories)
            .Query()
            .Include(mc => mc.Category)
            .Where(mc => !mc.Category.IsDeleted)
            .LoadAsync(cancellationToken);
    }

    // LINQ TO ENTITIES - складні запити з M:N
    public async Task<IEnumerable<Movie>> GetMoviesByCategoryAsync(long categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => !m.IsDeleted)
            .Where(m => m.MovieCategories.Any(mc => mc.CategoryId == categoryId && !mc.Category.IsDeleted))
            .Include(m => m.MovieCategories)
                .ThenInclude(mc => mc.Category)
            .OrderByDescending(m => m.Rating)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Movie>> SearchMoviesAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        return await _dbSet
            .Where(m => !m.IsDeleted)
            .Where(m => m.Title.ToLower().Contains(lowerSearchTerm) 
                     || m.OriginalTitle.ToLower().Contains(lowerSearchTerm)
                     || m.Director.ToLower().Contains(lowerSearchTerm)
                     || m.Description.ToLower().Contains(lowerSearchTerm))
            .Include(m => m.MovieCategories)
                .ThenInclude(mc => mc.Category)
            .OrderByDescending(m => m.Rating)
            .ThenBy(m => m.Title)
            .ToListAsync(cancellationToken);
    }

    // SPECIFICATION PATTERN
    public async Task<IEnumerable<Movie>> GetMoviesBySpecificationAsync(ISpecification<Movie> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    public async Task<int> CountBySpecificationAsync(ISpecification<Movie> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    private IQueryable<Movie> ApplySpecification(ISpecification<Movie> specification)
    {
        return SpecificationEvaluator.GetQuery(_dbSet, specification);
    }
}