using CatalogService.Data;
using CatalogService.Models;
using CatalogService.Repositories.Interfaces;
using CatalogService.Specifications;
using CatalogService.Specifications.ShowtimeSpecifications;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repositories;

public class ShowtimeRepository : GenericRepository<Showtime>, IShowtimeRepository
{
    public ShowtimeRepository(CatalogDbContext context) : base(context)
    {
    }

    // EAGER LOADING
    public async Task<IEnumerable<Showtime>> GetShowtimesByMovieAsync(long movieId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.MovieId == movieId && !s.IsDeleted && s.IsActive)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Showtime>> GetShowtimesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.StartTime >= startDate && s.StartTime < endDate && !s.IsDeleted && s.IsActive)
            .OrderBy(s => s.StartTime)
            .ThenBy(s => s.Hall.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Showtime>> GetActiveShowtimesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        
        return await _dbSet
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.StartTime > now && !s.IsDeleted && s.IsActive)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    // LINQ TO ENTITIES - складний запит
    public async Task<bool> IsHallAvailableAsync(long hallId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        return !await _dbSet
            .AnyAsync(s => s.HallId == hallId 
                        && !s.IsDeleted 
                        && s.IsActive
                        && ((s.StartTime >= startTime && s.StartTime < endTime)
                            || (s.EndTime > startTime && s.EndTime <= endTime)
                            || (s.StartTime <= startTime && s.EndTime >= endTime)), 
                      cancellationToken);
    }

    // SPECIFICATION PATTERN
    public async Task<IEnumerable<Showtime>> GetMoviesBySpecificationAsync(ISpecification<Showtime> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    public async Task<int> CountBySpecificationAsync(ISpecification<Showtime> specification, CancellationToken cancellationToken = default)
    {
        // Для підрахунку не застосовуємо пагінацію
        var specWithoutPaging = new ShowtimeCountSpecification(specification);
        return await ApplySpecification(specWithoutPaging).CountAsync(cancellationToken);
    }

    private IQueryable<Showtime> ApplySpecification(ISpecification<Showtime> specification)
    {
        return SpecificationEvaluator.GetQuery(_dbSet, specification);
    }

    // EXPLICIT LOADING
    public async Task LoadMovieAsync(Showtime showtime, CancellationToken cancellationToken = default)
    {
        await _context.Entry(showtime)
            .Reference(s => s.Movie)
            .LoadAsync(cancellationToken);
    }

    public async Task LoadHallAsync(Showtime showtime, CancellationToken cancellationToken = default)
    {
        await _context.Entry(showtime)
            .Reference(s => s.Hall)
            .LoadAsync(cancellationToken);
    }
}
