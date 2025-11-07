using BookingService.Domain.Models;

namespace BookingService.Domain.Models;

/// <summary>
/// Бронювання квитків
/// Зв'язок 1:N з Customer
/// </summary>
public class Booking
{
    public long BookingId { get; set; }
    public long CustomerId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; //Pending,Confirmed,Paid,Cancelled
    public string? PaymentMethod { get; set; }
    
    //Аудитні поля
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "System";
    public bool IsDeleted { get; set; }
    
    //Navigation properties
    public Customer? Customer { get; set; }
    public BookingDetails? Details { get; set; }
    public List<Ticket> Tickets { get; set; } = new();
    
    //Computed properties
    public decimal FinalAmount => TotalAmount - (Details?.DiscountAmount ?? 0);
    public int TicketCount => Tickets.Count;
}