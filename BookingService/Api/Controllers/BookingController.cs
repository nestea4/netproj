using System.Data;
using BookingService.Bll.DTOs;
using BookingService.Bll.Interfaces;
using BookingService.Dal.Interfaces;
using BookingService.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Api.Controllers;

/// <summary>
/// API для управління бронюваннями квитків
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingController> _logger;

    public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Створення нового бронювання з квитками (транзакційна операція)
    /// Unit of Work через BLL сервіс
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateBookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateBookingWithTickets(
        [FromBody] CreateBookingWithTicketsRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating booking with tickets for customer {CustomerId}", request.CustomerId);

        try
        {
            var response = await _bookingService.CreateBookingWithTicketsAsync(request, cancellationToken);

            _logger.LogInformation("Booking created successfully: {BookingNumber}", response.BookingNumber);

            return CreatedAtAction(
                nameof(GetBooking),
                new { id = response.BookingId },
                response
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new ErrorResponse 
            { 
                Error = "Resource not found",
                Details = ex.Message
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already taken"))
        {
            return Conflict(new ErrorResponse
            {
                Error = "Seat conflict",
                Details = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Validation failed",
                Details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking with tickets");
            return BadRequest(new ErrorResponse
            {
                Error = "Failed to create booking",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Підтвердження та оплата бронювання (транзакційна операція)
    /// </summary>
    [HttpPost("{id:long}/confirm-and-pay")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmAndPay(
        long id, 
        [FromBody] PaymentRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Confirming and paying booking {BookingId}", id);

        try
        {
            await _bookingService.ConfirmAndPayAsync(id, request.PaymentMethod, cancellationToken);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Booking confirmed and payment processed successfully",
                Data = new { BookingId = id, Status = "Paid", PaymentMethod = request.PaymentMethod }
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new ErrorResponse 
            { 
                Error = "Booking not found",
                Details = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ErrorResponse
            {
                Error = "Invalid operation",
                Details = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Validation failed",
                Details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming and paying booking");
            return BadRequest(new ErrorResponse
            {
                Error = "Transaction failed",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Підтвердження бронювання
    /// </summary>
    [HttpPost("{id:long}/confirm")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmBooking(long id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Confirming booking {BookingId}", id);

        try
        {
            await _bookingService.ConfirmBookingAsync(id, cancellationToken);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Booking confirmed successfully",
                Data = new { BookingId = id, Status = "Confirmed" }
            });
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found") 
                ? NotFound(new ErrorResponse { Error = "Booking not found", Details = ex.Message })
                : Conflict(new ErrorResponse { Error = "Invalid operation", Details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming booking");
            return BadRequest(new ErrorResponse { Error = "Failed to confirm booking", Details = ex.Message });
        }
    }

    /// <summary>
    /// Оплата бронювання
    /// </summary>
    [HttpPost("{id:long}/pay")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PayBooking(
        long id, 
        [FromBody] PaymentRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing payment for booking {BookingId}", id);

        try
        {
            await _bookingService.PayBookingAsync(id, request.PaymentMethod, cancellationToken);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Payment processed successfully",
                Data = new { BookingId = id, Status = "Paid", PaymentMethod = request.PaymentMethod }
            });
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found")
                ? NotFound(new ErrorResponse { Error = "Booking not found", Details = ex.Message })
                : Conflict(new ErrorResponse { Error = "Invalid operation", Details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return BadRequest(new ErrorResponse { Error = "Payment failed", Details = ex.Message });
        }
    }

    /// <summary>
    /// Отримання інформації про бронювання
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(BookingDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBooking(long id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting booking {BookingId}", id);

        try
        {
            var booking = await _bookingService.GetByIdAsync(id, cancellationToken);
            
            if (booking == null)
            {
                return NotFound(new ErrorResponse { Error = "Booking not found" });
            }

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
    [HttpGet("number/{bookingNumber}")]
    [ProducesResponseType(typeof(BookingDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookingByNumber(string bookingNumber, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _bookingService.GetByNumberAsync(bookingNumber, cancellationToken);
            
            if (booking == null)
            {
                return NotFound(new ErrorResponse { Error = "Booking not found" });
            }

            return Ok(booking);
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
    [HttpGet("customer/{customerId:long}")]
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerBookings(long customerId, CancellationToken cancellationToken)
    {
        try
        {
            var bookings = await _bookingService.GetCustomerBookingsAsync(customerId, cancellationToken);
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
    [HttpGet("{id:long}/history")]
    [ProducesResponseType(typeof(IEnumerable<BookingStatusHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookingHistory(long id, CancellationToken cancellationToken)
    {
        try
        {
            var history = await _bookingService.GetBookingHistoryAsync(id, cancellationToken);
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
    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelBooking(
        long id, 
        [FromBody] CancelBookingRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling booking {BookingId}", id);

        try
        {
            await _bookingService.CancelBookingAsync(id, request.Reason, cancellationToken);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Booking cancelled successfully",
                Data = new { BookingId = id, Status = "Cancelled" }
            });
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found")
                ? NotFound(new ErrorResponse { Error = "Booking not found", Details = ex.Message })
                : Conflict(new ErrorResponse { Error = "Invalid operation", Details = ex.Message });
        }
        catch (Exception ex)
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
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(IEnumerable<BookingStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTime? fromDate, 
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var from = fromDate ?? DateTime.Now.AddDays(-30);
            var to = toDate ?? DateTime.Now;

            var stats = await _bookingService.GetStatisticsAsync(from, to, cancellationToken);
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