using System.Data;
using BookingService.Bll.DTOs;
using BookingService.Dal;
using BookingService.Domain.Models;
using Dapper;

namespace BookingService.Data.Repositories;

/// <summary>
/// Репозиторій для роботи з бронюваннями
/// </summary>
public class BookingRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public BookingRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Створення нового бронювання через stored procedure
    /// </summary>
    public async Task<(long bookingId, string bookingNumber)> CreateBookingAsync(
        long customerId, decimal totalAmount, string createdBy = "API")
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("@p_CustomerId", customerId);
        parameters.Add("@p_TotalAmount", totalAmount);
        parameters.Add("@p_CreatedBy", createdBy);
        parameters.Add("@p_BookingId", dbType: DbType.Int64, direction: ParameterDirection.Output);
        parameters.Add("@p_BookingNumber", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "sp_CreateBooking", 
            parameters, 
            commandType: CommandType.StoredProcedure
        );

        var bookingId = parameters.Get<long>("@p_BookingId");
        var bookingNumber = parameters.Get<string>("@p_BookingNumber");

        return (bookingId, bookingNumber);
    }

    /// <summary>
    /// Додавання квитка до бронювання через stored procedure
    /// </summary>
    public async Task AddTicketAsync(long bookingId, AddTicketRequest ticket, string createdBy = "API")
    {
        using var connection = _connectionFactory.CreateConnection();
        
        await connection.ExecuteAsync("sp_AddTicketToBooking", new
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
        }, commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// Підтвердження бронювання через stored procedure
    /// </summary>
    public async Task ConfirmBookingAsync(long bookingId, string confirmedBy = "API")
    {
        using var connection = _connectionFactory.CreateConnection();
        
        await connection.ExecuteAsync("sp_ConfirmBooking", new
        {
            p_BookingId = bookingId,
            p_ConfirmedBy = confirmedBy
        }, commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// Оплата бронювання через stored procedure
    /// </summary>
    public async Task PayBookingAsync(long bookingId, string paymentMethod, string paidBy = "API")
    {
        using var connection = _connectionFactory.CreateConnection();
        
        await connection.ExecuteAsync("sp_PayBooking", new
        {
            p_BookingId = bookingId,
            p_PaymentMethod = paymentMethod,
            p_PaidBy = paidBy
        }, commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// Скасування бронювання через stored procedure
    /// </summary>
    public async Task CancelBookingAsync(long bookingId, string reason, string cancelledBy = "API")
    {
        using var connection = _connectionFactory.CreateConnection();
        
        await connection.ExecuteAsync("sp_CancelBooking", new
        {
            p_BookingId = bookingId,
            p_Reason = reason,
            p_CancelledBy = cancelledBy
        }, commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// Отримання повної інформації про бронювання через stored procedure
    /// </summary>
    public async Task<BookingDetailsResponse?> GetBookingWithTicketsAsync(long bookingId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        using var multi = await connection.QueryMultipleAsync(
            "sp_GetBookingWithTickets",
            new { p_BookingId = bookingId },
            commandType: CommandType.StoredProcedure
        );

        //читаю перший result set(booking info)
        var bookingData = await multi.ReadFirstOrDefaultAsync<dynamic>();
        if (bookingData == null)
            return null;

        //читаю другий result set(tickets)
        var tickets = await multi.ReadAsync<dynamic>();

        //маппінг в DTO
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

    /// <summary>
    /// Отримання всіх бронювань клієнта
    /// </summary>
    public async Task<IEnumerable<Booking>> GetCustomerBookingsAsync(long customerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT * FROM Booking 
            WHERE CustomerId = @CustomerId AND IsDeleted = FALSE 
            ORDER BY BookingDate DESC";

        return await connection.QueryAsync<Booking>(sql, new { CustomerId = customerId });
    }

    /// <summary>
    /// Пошук бронювання за номером
    /// </summary>
    public async Task<Booking?> GetBookingByNumberAsync(string bookingNumber)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = "SELECT * FROM Booking WHERE BookingNumber = @BookingNumber AND IsDeleted = FALSE";
        
        return await connection.QueryFirstOrDefaultAsync<Booking>(sql, new { BookingNumber = bookingNumber });
    }

    /// <summary>
    /// Отримання історії статусів бронювання
    /// </summary>
    public async Task<IEnumerable<BookingStatusHistory>> GetBookingHistoryAsync(long bookingId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT * FROM BookingStatusHistory 
            WHERE BookingId = @BookingId 
            ORDER BY ChangedAt ASC";

        return await connection.QueryAsync<BookingStatusHistory>(sql, new { BookingId = bookingId });
    }

    /// <summary>
    /// Статистика бронювань за період
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetBookingStatisticsAsync(DateTime fromDate, DateTime toDate)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                DATE(BookingDate) as Date,
                Status,
                COUNT(*) as Count,
                SUM(TotalAmount) as TotalRevenue
            FROM Booking
            WHERE BookingDate BETWEEN @FromDate AND @ToDate
              AND IsDeleted = FALSE
            GROUP BY DATE(BookingDate), Status
            ORDER BY Date DESC, Status";

        return await connection.QueryAsync(sql, new { FromDate = fromDate, ToDate = toDate });
    }
}