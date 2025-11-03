using BookingService.Bll.DTOs;
using BookingService.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace BookingService.Controllers;

/// <summary>
/// API для управління бронюваннями квитків
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BookingController : ControllerBase
{
    private readonly BookingRepository _bookingRepository;
    private readonly ILogger<BookingController> _logger;

    public BookingController(BookingRepository bookingRepository, ILogger<BookingController> logger)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    /// <summary>
    /// Створення нового бронювання
    /// </summary>
    /// <param name="request">Дані для створення бронювання</param>
    /// <returns>Інформація про створене бронювання</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateBookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        try
        {
            _logger.LogInformation("Creating booking for customer {CustomerId}", request.CustomerId);

            var (bookingId, bookingNumber) = await _bookingRepository.CreateBookingAsync(
                request.CustomerId,
                request.InitialAmount
            );

            var response = new CreateBookingResponse
            {
                BookingId = bookingId,
                BookingNumber = bookingNumber,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                Message = "Booking created successfully"
            };

            return CreatedAtAction(
                nameof(GetBooking),
                new { id = bookingId },
                response
            );
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, "Database error creating booking");
            return BadRequest(new ErrorResponse
            {
                Error = "Failed to create booking",
                Details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            return BadRequest(new ErrorResponse
            {
                Error = "An error occurred",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Додавання квитка до бронювання
    /// </summary>
    /// <param name="id">ID бронювання</param>
    /// <param name="request">Дані квитка</param>
    [HttpPost("{id}/tickets")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddTicket(long id, [FromBody] AddTicketRequest request)
    {
        try
        {
            _logger.LogInformation("Adding ticket to booking {BookingId}", id);

            await _bookingRepository.AddTicketAsync(id, request);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Ticket added successfully",
                Data = new { BookingId = id, Seat = $"{request.SeatRow}{request.SeatNumber}" }
            });
        }
        catch (MySqlException ex) when (ex.Message.Contains("Seat already taken"))
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Seat is already taken",
                Details = $"Seat {request.SeatRow}{request.SeatNumber} is not available"
            });
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, "Database error adding ticket");
            return BadRequest(new ErrorResponse
            {
                Error = "Failed to add ticket",
                Details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding ticket");
            return BadRequest(new ErrorResponse
            {
                Error = "An error occurred",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Підтвердження бронювання
    /// </summary>
    /// <param name="id">ID бронювання</param>
    [HttpPost("{id}/confirm")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmBooking(long id)
    {
        try
        {
            _logger.LogInformation("Confirming booking {BookingId}", id);

            await _bookingRepository.ConfirmBookingAsync(id);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Booking confirmed successfully",
                Data = new { BookingId = id, Status = "Confirmed" }
            });
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, "Error confirming booking");
            return BadRequest(new ErrorResponse
            {
                Error = "Failed to confirm booking",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Оплата бронювання
    /// </summary>
    /// <param name="id">ID бронювання</param>
    /// <param name="request">Спосіб оплати</param>
    [HttpPost("{id}/pay")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PayBooking(long id, [FromBody] PaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Processing payment for booking {BookingId}", id);

            await _bookingRepository.PayBookingAsync(id, request.PaymentMethod);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Payment processed successfully",
                Data = new { BookingId = id, Status = "Paid", PaymentMethod = request.PaymentMethod }
            });
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return BadRequest(new ErrorResponse
            {
                Error = "Payment failed",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Отримання інформації про бронювання
    /// </summary>
    /// <param name="id">ID бронювання</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookingDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBooking(long id)
    {
        try
        {
            _logger.LogInformation("Getting booking {BookingId}", id);

            var booking = await _bookingRepository.GetBookingWithTicketsAsync(id);
            
            if (booking == null)
                return NotFound(new ErrorResponse { Error = "Booking not found" });

            return Ok(booking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking");
            return BadRequest(new ErrorResponse
            {
                Error = "Failed to retrieve booking",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Отримання бронювання за номером
    /// </summary>
    /// <param name="bookingNumber">Номер бронювання</param>
    [HttpGet("number/{bookingNumber}")]
    [ProducesResponseType(typeof(BookingDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookingByNumber(string bookingNumber)
    {
        try
        {
            var booking = await _bookingRepository.GetBookingByNumberAsync(bookingNumber);
            
            if (booking == null)
                return NotFound(new ErrorResponse { Error = "Booking not found" });

            var details = await _bookingRepository.GetBookingWithTicketsAsync(booking.BookingId);
            return Ok(details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking by number");
            return BadRequest(new ErrorResponse
            {
                Error = "Failed to retrieve booking",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Отримання всіх бронювань клієнта
    /// </summary>
    /// <param name="customerId">ID клієнта</param>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerBookings(long customerId)
    {
        try
        {
            var bookings = await _bookingRepository.GetCustomerBookingsAsync(customerId);
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer bookings");
            return BadRequest(new ErrorResponse
            {
                Error = "Failed to retrieve bookings",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Отримання історії статусів бронювання
    /// </summary>
    /// <param name="id">ID бронювання</param>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookingHistory(long id)
    {
        try
        {
            var history = await _bookingRepository.GetBookingHistoryAsync(id);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking history");
            return BadRequest(new ErrorResponse
            {
                Error = "Failed to retrieve history",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Скасування бронювання
    /// </summary>
    /// <param name="id">ID бронювання</param>
    /// <param name="request">Причина скасування</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelBooking(long id, [FromBody] CancelBookingRequest request)
    {
        try
        {
            _logger.LogInformation("Cancelling booking {BookingId}", id);

            await _bookingRepository.CancelBookingAsync(id, request.Reason);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Booking cancelled successfully",
                Data = new { BookingId = id, Status = "Cancelled" }
            });
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, "Error cancelling booking");
            return BadRequest(new ErrorResponse
            {
                Error = "Failed to cancel booking",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Статистика бронювань за період
    /// </summary>
    /// <param name="fromDate">Початкова дата</param>
    /// <param name="toDate">Кінцева дата</param>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            var from = fromDate ?? DateTime.Now.AddDays(-30);
            var to = toDate ?? DateTime.Now;

            var stats = await _bookingRepository.GetBookingStatisticsAsync(from, to);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics");
            return BadRequest(new ErrorResponse
            {
                Error = "Failed to retrieve statistics",
                Details = ex.Message
            });
        }
    }
}