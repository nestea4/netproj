namespace BookingService.Domain.Models;

/// <summary>
/// Квиток на сеанс
/// Зв'язок M:N між Booking та Showtime
/// Дублює дані з Catalog Service для автономності
/// </summary>
public class Ticket
{
    public long TicketId { get; set; }
    public long BookingId { get; set; }
    
    //дані з Catalog Service
    public long ShowtimeId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime ShowDateTime { get; set; }
    public string HallName { get; set; } = string.Empty;
    
    //дані квитка
    public string SeatRow { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty;
    public decimal TicketPrice { get; set; }
    public string TicketType { get; set; } = "Adult"; // Adult,Child,Student,Senior
    
    //аудитні поля
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "System";
    public bool IsDeleted { get; set; }
    
    //Computed property
    public string FullSeat => $"{SeatRow}{SeatNumber}";
}