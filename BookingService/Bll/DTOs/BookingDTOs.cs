namespace BookingService.Bll.DTOs;

/// <summary>
/// Запит на створення бронювання з квитками
/// </summary>
public class CreateBookingWithTicketsRequest
{
    public long CustomerId { get; set; }
    public List<AddTicketRequest> Tickets { get; set; } = new();
}

/// <summary>
/// Відповідь після створення бронювання
/// </summary>
public class CreateBookingResponse
{
    public long BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<TicketInfo> Tickets { get; set; } = new();
}

/// <summary>
/// Спрощене DTO для списків бронювань
/// </summary>
public class BookingDto
{
    public long BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TicketCount { get; set; }
}

/// <summary>
/// Повна інформація про бронювання з деталями
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
/// Історія статусів бронювання
/// </summary>
public class BookingStatusHistoryDto
{
    public long HistoryId { get; set; }
    public long BookingId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

/// <summary>
/// Статистика бронювань
/// </summary>
public class BookingStatisticsDto
{
    public DateTime Date { get; set; }
    public int TotalBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int PaidBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal TotalRevenue { get; set; }
}