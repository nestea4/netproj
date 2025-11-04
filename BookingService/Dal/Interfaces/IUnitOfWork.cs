using System.Data;

namespace BookingService.Dal.Interfaces;

/// <summary>
///контракт для Unit of Work
/// </summary>
public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    ICustomerRepository Customers { get; }
    
    IBookingRepository Bookings { get; }
    
    ITicketRepository Tickets { get; }

    /// <summary>
    ///ознака активної транзакції
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    ///почати нову транзакцію з заданим рівнем ізоляції
    /// </summary>
    Task BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///зафіксувати всі зміни в базі
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///відкотити всі зміни
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}