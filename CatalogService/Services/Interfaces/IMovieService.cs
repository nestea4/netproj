using CatalogService.Models.DTOs;

namespace CatalogService.Services.Interfaces;

public interface IMovieService
{
    Task<PagedResult<MovieDto>> GetMoviesAsync(MovieFilterParameters filters, CancellationToken cancellationToken = default);
    Task<MovieDetailDto> GetMovieByIdAsync(long movieId, CancellationToken cancellationToken = default);
    Task<MovieDto> CreateMovieAsync(CreateMovieDto dto, CancellationToken cancellationToken = default);
    Task<MovieDto> UpdateMovieAsync(long movieId, UpdateMovieDto dto, CancellationToken cancellationToken = default);
    Task DeleteMovieAsync(long movieId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MovieDto>> GetPopularMoviesAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<MovieDto>> GetMoviesByCategoryAsync(long categoryId, CancellationToken cancellationToken = default);
}