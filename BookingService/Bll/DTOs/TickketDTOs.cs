namespace BookingService.Bll.DTOs;

// <summary>
/// Запит на додавання квитка до бронювання
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
/// Інформація про квиток в бронюванні
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