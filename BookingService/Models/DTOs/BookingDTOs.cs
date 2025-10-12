namespace BookingService.Models.DTOs;

/// <summary>
/// Запит на створення бронювання
/// </summary>
public class CreateBookingRequest
{
    public long CustomerId { get; set; }
    public decimal InitialAmount { get; set; } = 0;
}

/// <summary>
/// Відповідь після створення бронювання
/// </summary>
public class CreateBookingResponse
{
    public long BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = "Booking created successfully";
}

/// <summary>
/// Запит на додавання квитка
/// </summary>
public class AddTicketRequest
{
    public long ShowtimeId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime ShowDateTime { get; set; }
    public string HallName { get; set; } = string.Empty;
    public string SeatRow { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty;
    public decimal TicketPrice { get; set; }
    public string TicketType { get; set; } = "Adult";
}

/// <summary>
/// Запит на оплату
/// </summary>
public class PaymentRequest
{
    public string PaymentMethod { get; set; } = string.Empty; // CreditCard, Cash, DebitCard
}

/// <summary>
/// Запит на скасування
/// </summary>
public class CancelBookingRequest
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Відповідь з повною інформацією про бронювання
/// </summary>
public class BookingDetailsResponse
{
    public long BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    
    // Customer info
    public long CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    // Booking details
    public string? DiscountCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public bool ConfirmationEmailSent { get; set; }
    
    // Tickets
    public List<TicketInfo> Tickets { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Інформація про квиток
/// </summary>
public class TicketInfo
{
    public long TicketId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime ShowDateTime { get; set; }
    public string HallName { get; set; } = string.Empty;
    public string Seat { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Стандартна API відповідь
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

/// <summary>
/// Відповідь з помилкою
/// </summary>
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}