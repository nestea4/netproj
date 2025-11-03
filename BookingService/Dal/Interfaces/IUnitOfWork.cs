using System.Data;

namespace BookingService.Dal.Interfaces;

/// <summary>
/// Контракт для Unit of Work.
/// Координує транзакції між репозиторіями та забезпечує атомарність операцій.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Репозиторій для роботи з клієнтами.
    /// </summary>
    ICustomerRepository Customers { get; }

    /// <summary>
    /// Репозиторій для роботи з бронюваннями.
    /// </summary>
    IBookingRepository Bookings { get; }

    /// <summary>
    /// Репозиторій для роботи з квитками.
    /// </summary>
    ITicketRepository Tickets { get; }

    /// <summary>
    /// Ознака активної транзакції.
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// Почати нову транзакцію з заданим рівнем ізоляції.
    /// </summary>
    Task BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Зафіксувати всі зміни в базі.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Відкотити всі зміни.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}