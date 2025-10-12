namespace BookingService.Models;

/// <summary>
/// Клієнт кінотеатру
/// Root aggregate для контексту бронювання
/// </summary>
public class Customer
{
    public long CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    // Аудитні поля
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "System";
    public bool IsDeleted { get; set; }
    
    // Computed property
    public string FullName => $"{FirstName} {LastName}";
}