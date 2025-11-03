using System.Data;
using BookingService.Dal.Interfaces;
using BookingService.Dal.Repositories;
using MySqlConnector;

namespace BookingService.Dal.UnitOfWork;

/// <summary>
/// Unit of Work для координації транзакцій між репозиторіями
/// Забезпечує атомарність операцій та узгодженість даних
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DbConnectionFactory _connectionFactory;
    private MySqlConnection? _connection;
    private MySqlTransaction? _transaction;
    
    private ICustomerRepository? _customers;
    private IBookingRepository? _bookings;
    private ITicketRepository? _tickets;

    private bool _disposed;

    public UnitOfWork(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <summary>
    /// Lazy initialization репозиторіїв з прив'язкою до спільного з'єднання
    /// </summary>
    public ICustomerRepository Customers
    {
        get
        {
            if (_customers == null)
            {
                var repository = new CustomerRepository(_connectionFactory);
                if (_connection != null)
                {
                    repository.SetSharedConnection(_connection, _transaction);
                }
                _customers = repository;
            }
            return _customers;
        }
    }

    public IBookingRepository Bookings
    {
        get
        {
            if (_bookings == null)
            {
                var repository = new BookingRepository(_connectionFactory);
                if (_connection != null)
                {
                    repository.SetSharedConnection(_connection, _transaction);
                }
                _bookings = repository;
            }
            return _bookings;
        }
    }

    public ITicketRepository Tickets
    {
        get
        {
            if (_tickets == null)
            {
                var repository = new TicketRepository(_connectionFactory);
                if (_connection != null)
                {
                    repository.SetSharedConnection(_connection, _transaction);
                }
                _tickets = repository;
            }
            return _tickets;
        }
    }

    public bool HasActiveTransaction => _transaction != null;

    /// <summary>
    /// Початок транзакції з вказаним рівнем ізоляції
    /// 
    /// Рівні ізоляції:
    /// - ReadCommitted (default): Баланс між консистентністю та performance
    /// - ReadUncommitted: Найшвидший, але можливі dirty reads
    /// - RepeatableRead: Захищає від non-repeatable reads, але можливі phantom reads
    /// - Serializable: Найсуворіший, але найповільніший (можливі deadlocks)
    /// 
    /// Trade-offs:
    /// - ReadCommitted: добре для більшості OLTP сценаріїв (створення бронювань)
    /// - RepeatableRead/Serializable: для критичних операцій (платежі, резервування місць)
    /// </summary>
    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transaction already started");
        }

        _connection = _connectionFactory.CreateConnection();
        
        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken);
        }

        _transaction = await _connection.BeginTransactionAsync(isolationLevel, cancellationToken);

        // Прив'язуємо існуючі репозиторії до нової транзакції
        if (_customers is CustomerRepository customerRepo)
        {
            customerRepo.SetSharedConnection(_connection, _transaction);
        }
        if (_bookings is BookingRepository bookingRepo)
        {
            bookingRepo.SetSharedConnection(_connection, _transaction);
        }
        if (_tickets is TicketRepository ticketRepo)
        {
            ticketRepo.SetSharedConnection(_connection, _transaction);
        }
    }

    /// <summary>
    /// Фіксація всіх змін в базі даних
    /// </summary>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit");
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <summary>
    /// Відкат всіх змін
    /// </summary>
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to rollback");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _connection?.Dispose();
            _disposed = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }

            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }

            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}