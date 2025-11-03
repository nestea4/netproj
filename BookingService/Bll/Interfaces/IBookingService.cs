using BookingService.Bll.DTOs;

namespace BookingService.Bll.Interfaces;

// <summary>
/// Інтерфейс сервісу для роботи з бронюваннями
/// Координує складні транзакційні операції через UoW
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Отримання бронювання за ID з повними деталями
    /// </summary>
    Task<BookingDetailsResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримання бронювання за номером
    /// </summary>
    Task<BookingDetailsResponse?> GetByNumberAsync(string bookingNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримання всіх бронювань клієнта
    /// </summary>
    Task<IEnumerable<BookingDto>> GetCustomerBookingsAsync(long customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Створення бронювання з квитками (транзакційна операція)
    /// Демонструє використання UoW для атомарності
    /// </summary>
    Task<CreateBookingResponse> CreateBookingWithTicketsAsync(
        CreateBookingWithTicketsRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Підтвердження бронювання
    /// </summary>
    Task ConfirmBookingAsync(long bookingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Оплата бронювання
    /// </summary>
    Task PayBookingAsync(long bookingId, string paymentMethod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Підтвердження та оплата в одній транзакції
    /// </summary>
    Task ConfirmAndPayAsync(long bookingId, string paymentMethod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Скасування бронювання
    /// </summary>
    Task CancelBookingAsync(long bookingId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримання історії статусів бронювання
    /// </summary>
    Task<IEnumerable<BookingStatusHistoryDto>> GetBookingHistoryAsync(long bookingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Статистика бронювань
    /// </summary>
    Task<IEnumerable<BookingStatisticsDto>> GetStatisticsAsync(
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);
}