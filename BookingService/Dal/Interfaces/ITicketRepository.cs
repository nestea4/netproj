using BookingService.Domain.Models;

namespace BookingService.Dal.Interfaces;

/// <summary>
/// Інтерфейс репозиторію для Ticket (ADO.NET + Dapper)
/// </summary>
public interface ITicketRepository : IRepository<Ticket>
{
    Task<IEnumerable<Ticket>> GetTicketsByBookingIdAsync(long bookingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetTicketsByShowtimeAsync(long showtimeId, CancellationToken cancellationToken = default);
    Task<bool> IsSeatAvailableAsync(long showtimeId, string seatRow, string seatNumber, CancellationToken cancellationToken = default);
    Task<int> GetBookedSeatsCountAsync(long showtimeId, CancellationToken cancellationToken = default);
}