using CatalogService.Models;
using CatalogService.Specifications;

namespace CatalogService.Repositories.Interfaces;

public interface IShowtimeRepository : IGenericRepository<Showtime>
{
    Task<IEnumerable<Showtime>> GetShowtimesByMovieAsync(long movieId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Showtime>> GetShowtimesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Showtime>> GetActiveShowtimesAsync(CancellationToken cancellationToken = default);
    Task<bool> IsHallAvailableAsync(long hallId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
    
    // Specification pattern
    Task<IEnumerable<Showtime>> GetMoviesBySpecificationAsync(ISpecification<Showtime> specification, CancellationToken cancellationToken = default);
    Task<int> CountBySpecificationAsync(ISpecification<Showtime> specification, CancellationToken cancellationToken = default);
    
    // Explicit Loading
    Task LoadMovieAsync(Showtime showtime, CancellationToken cancellationToken = default);
    Task LoadHallAsync(Showtime showtime, CancellationToken cancellationToken = default);
}