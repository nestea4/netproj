using CatalogService.Models.DTOs;

namespace CatalogService.Services.Interfaces;

public interface IShowtimeService
{
    Task<PagedResult<ShowtimeDto>> GetShowtimesAsync(long? movieId, DateTime? date, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<ShowtimeDto> GetShowtimeByIdAsync(long showtimeId, CancellationToken cancellationToken = default);
    Task<ShowtimeDto> CreateShowtimeAsync(CreateShowtimeDto dto, CancellationToken cancellationToken = default);
    Task DeleteShowtimeAsync(long showtimeId, CancellationToken cancellationToken = default);
    Task<bool> UpdateSeatsAsync(long showtimeId, int seatsToReserve, CancellationToken cancellationToken = default);
}