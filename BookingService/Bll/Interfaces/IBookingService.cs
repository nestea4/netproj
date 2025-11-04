using BookingService.Bll.DTOs;

namespace BookingService.Bll.Interfaces;

// <summary>
///Інтерфейс сервісу для роботи з бронюваннями
///складні транзакційні операції через UoW
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// отримання бронювання за ID з повними деталями
    /// </summary>
    Task<BookingDetailsResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// отримання бронювання за номером
    /// </summary>
    Task<BookingDetailsResponse?> GetByNumberAsync(string bookingNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// отримання всіх бронювань клієнта
    /// </summary>
    Task<IEnumerable<BookingDto>> GetCustomerBookingsAsync(long customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// створення бронювання з квитками (транзакційна операція)
    /// використання UoW для атомарності
    /// </summary>
    Task<CreateBookingResponse> CreateBookingWithTicketsAsync(
        CreateBookingWithTicketsRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    ///підтвердження бронювання
    /// </summary>
    Task ConfirmBookingAsync(long bookingId, CancellationToken cancellationToken = default);

    /// <summary>
    ///оплата бронювання
    /// </summary>
    Task PayBookingAsync(long bookingId, string paymentMethod, CancellationToken cancellationToken = default);

    /// <summary>
    ///підтвердження та оплата в одній транзакції
    /// </summary>
    Task ConfirmAndPayAsync(long bookingId, string paymentMethod, CancellationToken cancellationToken = default);

    /// <summary>
    ///скасування бронювання
    /// </summary>
    Task CancelBookingAsync(long bookingId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    ///отримання історії статусів бронювання
    /// </summary>
    Task<IEnumerable<BookingStatusHistoryDto>> GetBookingHistoryAsync(long bookingId, CancellationToken cancellationToken = default);

    /// <summary>
    ///статистика бронювань
    /// </summary>
    Task<IEnumerable<BookingStatisticsDto>> GetStatisticsAsync(
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);
}