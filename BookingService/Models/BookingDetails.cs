namespace BookingService.Models;

/// <summary>
/// Деталі бронювання
/// Зв'язок 1:1 з Booking
/// </summary>
public class BookingDetails
{
    public long BookingDetailsId { get; set; }
    public long BookingId { get; set; }
    public string? DiscountCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? Notes { get; set; }
    public bool ConfirmationEmailSent { get; set; }
    public bool ReminderEmailSent { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "System";
}