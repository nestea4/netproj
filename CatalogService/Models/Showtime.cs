namespace CatalogService.Models;

/// <summary>
/// Сеанс показу фільму
/// Зв'язок: Movie (1:N) Showtime, Hall (1:N) Showtime
/// </summary>
public class Showtime
{
    public long ShowtimeId { get; set; }
    public long MovieId { get; set; }
    public long HallId { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public int AvailableSeats { get; set; }
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation
    public Movie Movie { get; set; } = null!;
    public Hall Hall { get; set; } = null!;
    
    // Computed property
    public bool IsPastShowtime => StartTime < DateTime.Now;
    public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;
}