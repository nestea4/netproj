using BookingService.Bll.DTOs;
using BookingService.Domain.Models;

namespace BookingService.Dal.Interfaces;

/// <summary>
/// Інтерфейс репозиторію для Booking (ADO.NET + Dapper)
/// </summary>
public interface IBookingRepository : IRepository<Booking>
{
    Task<(long bookingId, string bookingNumber)> CreateBookingAsync(
        long customerId, 
        decimal totalAmount, 
        string createdBy = "API",
        CancellationToken cancellationToken = default);
    
    Task AddTicketAsync(
        long bookingId, 
        AddTicketRequest ticket, 
        string createdBy = "API",
        CancellationToken cancellationToken = default);
    
    Task ConfirmBookingAsync(long bookingId, string confirmedBy = "API", CancellationToken cancellationToken = default);
    Task PayBookingAsync(long bookingId, string paymentMethod, string paidBy = "API", CancellationToken cancellationToken = default);
    Task CancelBookingAsync(long bookingId, string reason, string cancelledBy = "API", CancellationToken cancellationToken = default);
    
    Task<BookingDetailsResponse?> GetBookingWithTicketsAsync(long bookingId, CancellationToken cancellationToken = default);
    Task<Booking?> GetBookingByNumberAsync(string bookingNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetCustomerBookingsAsync(long customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BookingStatusHistory>> GetBookingHistoryAsync(long bookingId, CancellationToken cancellationToken = default);
}