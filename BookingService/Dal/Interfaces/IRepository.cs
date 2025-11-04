namespace BookingService.Dal.Interfaces;

/// <summary>
///generic repository interface для базових CRUD операцій
/// </summary>
/// <typeparam name="T">Тип сутності</typeparam>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<long> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(long id, CancellationToken cancellationToken = default);
}