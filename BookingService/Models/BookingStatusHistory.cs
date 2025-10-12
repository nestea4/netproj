namespace BookingService.Models;

/// <summary>
/// Історія змін статусів бронювання (аудит)
/// Зв'язок 1:N з Booking
/// </summary>
public class BookingStatusHistory
{
    public long HistoryId { get; set; }
    public long BookingId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = "System";
    public string? Reason { get; set; }
    public string? IpAddress { get; set; }
}