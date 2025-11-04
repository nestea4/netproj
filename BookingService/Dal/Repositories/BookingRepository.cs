using System.Data;
using BookingService.Bll.DTOs;
using BookingService.Dal.Interfaces;
using BookingService.Domain.Models;
using Dapper;
using MySqlConnector;

namespace BookingService.Dal.Repositories;

// <summary>
/// Репозиторій для роботи з бронюваннями (ADO.NET + Dapper)
/// </summary>
public class BookingRepository : IBookingRepository
{
    private readonly DbConnectionFactory _connectionFactory;
    private MySqlConnection? _sharedConnection;
    private MySqlTransaction? _transaction;

    public BookingRepository(DbConnectionFactory connectionFactory)
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

    #region Generic CRUD (використовує Dapper)

    public async Task<Booking?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        
        var sql = "SELECT * FROM Booking WHERE BookingId = @Id AND IsDeleted = FALSE";
        
        return await connection.QueryFirstOrDefaultAsync<Booking>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken)
        );
    }

    public async Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        
        var sql = @"
            SELECT * FROM Booking 
            WHERE IsDeleted = FALSE 
            ORDER BY BookingDate DESC 
            LIMIT 1000";
        
        return await connection.QueryAsync<Booking>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken)
        );
    }

    public async Task<long> CreateAsync(Booking entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        using var connection = GetConnection();
        
        var sql = @"
            INSERT INTO Booking (CustomerId, BookingNumber, BookingDate, TotalAmount, Status, 
                               PaymentMethod, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
            VALUES (@CustomerId, @BookingNumber, @BookingDate, @TotalAmount, @Status,
                    @PaymentMethod, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy, FALSE);
            SELECT LAST_INSERT_ID();";

        return await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken)
        );
    }

    public async Task<bool> UpdateAsync(Booking entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        using var connection = GetConnection();
        
        var sql = @"
            UPDATE Booking 
            SET TotalAmount = @TotalAmount,
                Status = @Status,
                PaymentMethod = @PaymentMethod,
                UpdatedAt = @UpdatedAt,
                UpdatedBy = @UpdatedBy
            WHERE BookingId = @BookingId AND IsDeleted = FALSE";

        var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken)
        );
        
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        
        var sql = "DELETE FROM Booking WHERE BookingId = @Id";
        
        var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken)
        );
        
        return rows > 0;
    }

    public async Task<bool> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        
        var sql = @"
            UPDATE Booking 
            SET IsDeleted = TRUE, 
                UpdatedAt = @UpdatedAt,
                UpdatedBy = @UpdatedBy
            WHERE BookingId = @Id";

        var rows = await connection.ExecuteAsync(
            new CommandDefinition(
                sql, 
                new { Id = id, UpdatedAt = DateTime.Now, UpdatedBy = "System" }, 
                _transaction, 
                cancellationToken: cancellationToken
            )
        );
        
        return rows > 0;
    }

    #endregion

    #region Business Operations (Stored Procedures)

    public async Task<(long bookingId, string bookingNumber)> CreateBookingAsync(
        long customerId, decimal totalAmount, string createdBy = "API", CancellationToken cancellationToken = default)
    {
        var connection = GetConnection();
        var shouldDispose = ShouldDisposeConnection();

        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_CustomerId", customerId);
            parameters.Add("@p_TotalAmount", totalAmount);
            parameters.Add("@p_CreatedBy", createdBy);
            parameters.Add("@p_BookingId", dbType: DbType.Int64, direction: ParameterDirection.Output);
            parameters.Add("@p_BookingNumber", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                new CommandDefinition(
                    "sp_CreateBooking",
                    parameters,
                    _transaction,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken
                )
            );

            var bookingId = parameters.Get<long>("@p_BookingId");
            var bookingNumber = parameters.Get<string>("@p_BookingNumber");

            return (bookingId, bookingNumber);
        }
        finally
        {
            if (shouldDispose)
            {
                await connection.DisposeAsync();
            }
        }
    }

    public async Task AddTicketAsync(long bookingId, AddTicketRequest ticket, string createdBy = "API", CancellationToken cancellationToken = default)
    {
        var connection = GetConnection();
        var shouldDispose = ShouldDisposeConnection();

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    "sp_AddTicketToBooking",
                    new
                    {
                        p_BookingId = bookingId,
                        p_ShowtimeId = ticket.ShowtimeId,
                        p_MovieTitle = ticket.MovieTitle,
                        p_ShowDateTime = ticket.ShowDateTime,
                        p_HallName = ticket.HallName,
                        p_SeatRow = ticket.SeatRow,
                        p_SeatNumber = ticket.SeatNumber,
                        p_TicketPrice = ticket.TicketPrice,
                        p_TicketType = ticket.TicketType,
                        p_CreatedBy = createdBy
                    },
                    _transaction,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken
                )
            );
        }
        finally
        {
            if (shouldDispose)
            {
                await connection.DisposeAsync();
            }
        }
    }

    public async Task ConfirmBookingAsync(long bookingId, string confirmedBy = "API", CancellationToken cancellationToken = default)
    {
        var connection = GetConnection();
        var shouldDispose = ShouldDisposeConnection();

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    "sp_ConfirmBooking",
                    new { p_BookingId = bookingId, p_ConfirmedBy = confirmedBy },
                    _transaction,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken
                )
            );
        }
        finally
        {
            if (shouldDispose)
            {
                await connection.DisposeAsync();
            }
        }
    }

    public async Task PayBookingAsync(long bookingId, string paymentMethod, string paidBy = "API", CancellationToken cancellationToken = default)
    {
        var connection = GetConnection();
        var shouldDispose = ShouldDisposeConnection();

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    "sp_PayBooking",
                    new { p_BookingId = bookingId, p_PaymentMethod = paymentMethod, p_PaidBy = paidBy },
                    _transaction,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken
                )
            );
        }
        finally
        {
            if (shouldDispose)
            {
                await connection.DisposeAsync();
            }
        }
    }

    public async Task CancelBookingAsync(long bookingId, string reason, string cancelledBy = "API", CancellationToken cancellationToken = default)
    {
        var connection = GetConnection();
        var shouldDispose = ShouldDisposeConnection();

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    "sp_CancelBooking",
                    new { p_BookingId = bookingId, p_Reason = reason, p_CancelledBy = cancelledBy },
                    _transaction,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken
                )
            );
        }
        finally
        {
            if (shouldDispose)
            {
                await connection.DisposeAsync();
            }
        }
    }

    #endregion

    #region Query Operations (Dapper)

    public async Task<BookingDetailsResponse?> GetBookingWithTicketsAsync(long bookingId, CancellationToken cancellationToken = default)
    {
        var connection = GetConnection();
        var shouldDispose = ShouldDisposeConnection();

        try
        {
            using var multi = await connection.QueryMultipleAsync(
                new CommandDefinition(
                    "sp_GetBookingWithTickets",
                    new { p_BookingId = bookingId },
                    _transaction,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken
                )
            );

            var bookingData = await multi.ReadFirstOrDefaultAsync<dynamic>();
            if (bookingData == null)
                return null;

            var tickets = await multi.ReadAsync<dynamic>();

            var response = new BookingDetailsResponse
            {
                BookingId = bookingData.BookingId,
                BookingNumber = bookingData.BookingNumber,
                BookingDate = bookingData.BookingDate,
                TotalAmount = bookingData.TotalAmount,
                Status = bookingData.Status,
                PaymentMethod = bookingData.PaymentMethod,
                CustomerId = bookingData.CustomerId,
                CustomerName = $"{bookingData.FirstName} {bookingData.LastName}",
                Email = bookingData.Email,
                Phone = bookingData.Phone,
                DiscountCode = bookingData.DiscountCode,
                DiscountAmount = bookingData.DiscountAmount ?? 0,
                FinalAmount = bookingData.FinalAmount,
                ConfirmationEmailSent = bookingData.ConfirmationEmailSent,
                CreatedAt = bookingData.CreatedAt,
                UpdatedAt = bookingData.UpdatedAt,
                Tickets = tickets.Select(t => new TicketInfo
                {
                    TicketId = t.TicketId,
                    MovieTitle = t.MovieTitle,
                    ShowDateTime = t.ShowDateTime,
                    HallName = t.HallName,
                    Seat = t.FullSeat,
                    Price = t.TicketPrice,
                    Type = t.TicketType
                }).ToList()
            };

            return response;
        }
        finally
        {
            if (shouldDispose)
            {
                await connection.DisposeAsync();
            }
        }
    }

    public async Task<Booking?> GetBookingByNumberAsync(string bookingNumber, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        
        var sql = "SELECT * FROM Booking WHERE BookingNumber = @BookingNumber AND IsDeleted = FALSE";
        
        return await connection.QueryFirstOrDefaultAsync<Booking>(
            new CommandDefinition(sql, new { BookingNumber = bookingNumber }, _transaction, cancellationToken: cancellationToken)
        );
    }

    public async Task<IEnumerable<Booking>> GetCustomerBookingsAsync(long customerId, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        
        var sql = @"
            SELECT * FROM Booking 
            WHERE CustomerId = @CustomerId AND IsDeleted = FALSE 
            ORDER BY BookingDate DESC";

        return await connection.QueryAsync<Booking>(
            new CommandDefinition(sql, new { CustomerId = customerId }, _transaction, cancellationToken: cancellationToken)
        );
    }

    public async Task<IEnumerable<BookingStatusHistory>> GetBookingHistoryAsync(long bookingId, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        
        var sql = @"
            SELECT * FROM BookingStatusHistory 
            WHERE BookingId = @BookingId 
            ORDER BY ChangedAt ASC";

        return await connection.QueryAsync<BookingStatusHistory>(
            new CommandDefinition(sql, new { BookingId = bookingId }, _transaction, cancellationToken: cancellationToken)
        );
    }

    #endregion
}