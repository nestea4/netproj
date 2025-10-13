namespace CatalogService.Models;

/// <summary>
/// Фільм (Root Aggregate для Catalog Context)
/// </summary>
public class Movie
{
    public long MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalTitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Director { get; set; } = string.Empty;
    public decimal Rating { get; set; } // IMDb rating
    public string PosterUrl { get; set; } = string.Empty;
    public string TrailerUrl { get; set; } = string.Empty;
    
    // Аудитні поля
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "System";
    public bool IsDeleted { get; set; }
    
    // Navigation properties
    public MovieDetails? Details { get; set; } // 1:1
    public ICollection<MovieCategory> MovieCategories { get; set; } = new List<MovieCategory>(); // M:N
    public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>(); // 1:N
}