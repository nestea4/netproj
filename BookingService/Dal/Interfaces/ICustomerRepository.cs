using BookingService.Domain.Models;

namespace BookingService.Dal.Interfaces;

/// <summary>
/// Інтерфейс репозиторію для Customer (чистий ADO.NET)
/// </summary>
public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
}