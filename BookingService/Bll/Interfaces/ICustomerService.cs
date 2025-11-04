using BookingService.Bll.DTOs;

namespace BookingService.Bll.Interfaces;

/// <summary>
/// Інтерфейс сервісу для роботи з клієнтами
/// бізнес-операції над Customer aggregate
/// </summary>
public interface ICustomerService
{
    /// <summary>
    ///отримання клієнта за ID
    /// </summary>
    Task<CustomerDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    ///отримання всіх клієнтів (з пагінацією)
    /// </summary>
    Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///створення нового клієнта
    /// </summary>
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///оновлення даних клієнта
    /// </summary>
    Task<CustomerDto> UpdateAsync(long id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///видалення клієнта (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    ///пошук клієнта за email
    /// </summary>
    Task<CustomerDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    ///пошук клієнтів за ім'ям
    /// </summary>
    Task<IEnumerable<CustomerDto>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    ///перевірка унікальності email
    /// </summary>
    Task<bool> IsEmailUniqueAsync(string email, long? excludeCustomerId = null, CancellationToken cancellationToken = default);
}