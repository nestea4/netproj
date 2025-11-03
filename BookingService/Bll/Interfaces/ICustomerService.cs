using BookingService.Bll.DTOs;

namespace BookingService.Bll.Interfaces;

/// <summary>
/// Інтерфейс сервісу для роботи з клієнтами
/// Визначає бізнес-операції над Customer aggregate
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Отримання клієнта за ID
    /// </summary>
    Task<CustomerDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримання всіх клієнтів (з пагінацією)
    /// </summary>
    Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Створення нового клієнта
    /// </summary>
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Оновлення даних клієнта
    /// </summary>
    Task<CustomerDto> UpdateAsync(long id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Видалення клієнта (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Пошук клієнта за email
    /// </summary>
    Task<CustomerDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Пошук клієнтів за ім'ям
    /// </summary>
    Task<IEnumerable<CustomerDto>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Перевірка унікальності email
    /// </summary>
    Task<bool> IsEmailUniqueAsync(string email, long? excludeCustomerId = null, CancellationToken cancellationToken = default);
}