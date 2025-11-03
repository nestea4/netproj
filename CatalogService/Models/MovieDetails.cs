namespace CatalogService.Models;

/// <summary>
/// Додаткові деталі фільму (1:1 з Movie)
/// </summary>
public class MovieDetails
{
    public long MovieDetailsId { get; set; }
    public long MovieId { get; set; } //FK для 1:1
    
    public string Country { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Budget { get; set; } = string.Empty;
    public string BoxOffice { get; set; } = string.Empty;
    public string Cast { get; set; } = string.Empty; //JSON або comma-separated
    public string AgeRating { get; set; } = string.Empty; //PG-13, R,ітд
    public string Awards { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    //Navigation
    public Movie Movie { get; set; } = null!;
}