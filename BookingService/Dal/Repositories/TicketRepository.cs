using System.Data;
using BookingService.Dal.Interfaces;
using BookingService.Domain.Models;
using Dapper;
using MySqlConnector;

namespace BookingService.Dal.Repositories;

/// <summary>
/// Репозиторій для роботи з квитками (ADO.NET + Dapper)
/// </summary>
public class TicketRepository : ITicketRepository
{
    private readonly DbConnectionFactory _connectionFactory;
    private MySqlConnection? _sharedConnection;
    private MySqlTransaction? _transaction;

    public TicketRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public void SetSharedConnection(MySqlConnection connection, MySqlTransaction? transaction = null)
    {
        _sharedConnection = connection;
        _transaction = transaction;
    }

    private MySqlConnection GetConnection()
    {
        return _sharedConnection ?? _connectionFactory.CreateConnection();
    }

    private bool ShouldDisposeConnection()
    {
        return _sharedConnection == null;
    }

    #region Generic CRUD

    public async Task<Ticket?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        
        var sql = "SELECT * FROM Ticket WHERE TicketId = @Id AND IsDeleted = FALSE";
        
        return await connection.QueryFirstOrDefaultAsync<Ticket>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken)
        );
    }

    public async Task<IEnumerable<Ticket>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        
        var sql = @"
            SELECT * FROM Ticket 
            WHERE IsDeleted = FALSE 
            ORDER BY CreatedAt DESC 
            LIMIT 1000";
        
        return await connection.QueryAsync<Ticket>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken)
        );
    }

    public async Task<long> CreateAsync(Ticket entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var connection = GetConnection();
        var shouldDispose = ShouldDisposeConnection();

        try
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            // Використовуємо ADO.NET для демонстрації ручної роботи
            using var command = connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = @"
                INSERT INTO Ticket (BookingId, ShowtimeId, MovieTitle, ShowDateTime, HallName,
                                  SeatRow, SeatNumber, TicketPrice, TicketType, 
                                  CreatedAt, CreatedBy, IsDeleted)
                VALUES (@BookingId, @ShowtimeId, @MovieTitle, @ShowDateTime, @HallName,
                        @SeatRow, @SeatNumber, @TicketPrice, @TicketType,
                        @CreatedAt, @CreatedBy, FALSE);
                SELECT LAST_INSERT_ID();";

            command.Parameters.Add(new MySqlParameter("@BookingId", MySqlDbType.Int64) { Value = entity.BookingId });
            command.Parameters.Add(new MySqlParameter("@ShowtimeId", MySqlDbType.Int64) { Value = entity.ShowtimeId });
            command.Parameters.Add(new MySqlParameter("@MovieTitle", MySqlDbType.VarChar, 200) { Value = entity.MovieTitle });
            command.Parameters.Add(new MySqlParameter("@ShowDateTime", MySqlDbType.DateTime) { Value = entity.ShowDateTime });
            command.Parameters.Add(new MySqlParameter("@HallName", MySqlDbType.VarChar, 100) { Value = entity.HallName });
            command.Parameters.Add(new MySqlParameter("@SeatRow", MySqlDbType.VarChar, 5) { Value = entity.SeatRow });
            command.Parameters.Add(new MySqlParameter("@SeatNumber", MySqlDbType.VarChar, 5) { Value = entity.SeatNumber });
            command.Parameters.Add(new MySqlParameter("@TicketPrice", MySqlDbType.Decimal) { Value = entity.TicketPrice });
            command.Parameters.Add(new MySqlParameter("@TicketType", MySqlDbType.VarChar, 50) { Value = entity.TicketType });
            command.Parameters.Add(new MySqlParameter("@CreatedAt", MySqlDbType.DateTime) { Value = DateTime.Now });
            command.Parameters.Add(new MySqlParameter("@CreatedBy", MySqlDbType.VarChar, 100) { Value = entity.CreatedBy });

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt64(result);
        }
        finally
        {
            if (shouldDispose)
            {
                await connection.DisposeAsync();
            }
        }
    }

    public async Task<bool> UpdateAsync(Ticket entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        using var connection = GetConnection();
        
        var sql = @"
            UPDATE Ticket 
            SET TicketPrice = @TicketPrice,
                TicketType = @TicketType
            WHERE TicketId = @TicketId AND IsDeleted = FALSE";

        var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken)
        );
        
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        
        var sql = "DELETE FROM Ticket WHERE TicketId = @Id";
        
        var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken)
        );
        
        return rows > 0;
    }

    public async Task<bool> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        
        var sql = "UPDATE Ticket SET IsDeleted = TRUE WHERE TicketId = @Id";

        var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken)
        );
        
        return rows > 0;
    }

    #endregion

    #region Specific Operations (Dapper)

    public async Task<IEnumerable<Ticket>> GetTicketsByBookingIdAsync(long bookingId, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        
        var sql = @"
            SELECT * FROM Ticket 
            WHERE BookingId = @BookingId AND IsDeleted = FALSE
            ORDER BY SeatRow, SeatNumber";

        return await connection.QueryAsync<Ticket>(
            new CommandDefinition(sql, new { BookingId = bookingId }, _transaction, cancellationToken: cancellationToken)
        );
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByShowtimeAsync(long showtimeId, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        
        var sql = @"
            SELECT t.* FROM Ticket t
            INNER JOIN Booking b ON t.BookingId = b.BookingId
            WHERE t.ShowtimeId = @ShowtimeId 
              AND t.IsDeleted = FALSE
              AND b.Status IN ('Confirmed', 'Paid')
              AND b.IsDeleted = FALSE
            ORDER BY t.SeatRow, t.SeatNumber";

        return await connection.QueryAsync<Ticket>(
            new CommandDefinition(sql, new { ShowtimeId = showtimeId }, _transaction, cancellationToken: cancellationToken)
        );
    }

    public async Task<bool> IsSeatAvailableAsync(long showtimeId, string seatRow, string seatNumber, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        
        var sql = @"
            SELECT COUNT(*) 
            FROM Ticket t
            INNER JOIN Booking b ON t.BookingId = b.BookingId
            WHERE t.ShowtimeId = @ShowtimeId 
              AND t.SeatRow = @SeatRow 
              AND t.SeatNumber = @SeatNumber
              AND t.IsDeleted = FALSE
              AND b.Status IN ('Pending', 'Confirmed', 'Paid')
              AND b.IsDeleted = FALSE";

        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { ShowtimeId = showtimeId, SeatRow = seatRow, SeatNumber = seatNumber }, 
                _transaction, cancellationToken: cancellationToken)
        );

        return count == 0;
    }

    public async Task<int> GetBookedSeatsCountAsync(long showtimeId, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        
        var sql = @"
            SELECT COUNT(*) 
            FROM Ticket t
            INNER JOIN Booking b ON t.BookingId = b.BookingId
            WHERE t.ShowtimeId = @ShowtimeId 
              AND t.IsDeleted = FALSE
              AND b.Status IN ('Confirmed', 'Paid')
              AND b.IsDeleted = FALSE";

        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { ShowtimeId = showtimeId }, _transaction, cancellationToken: cancellationToken)
        );
    }

    #endregion
}