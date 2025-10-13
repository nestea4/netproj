namespace CatalogService.Models;

/// <summary>
/// Проміжна таблиця для M:N між Movie та Category
/// </summary>
public class MovieCategory
{
    public long MovieId { get; set; }
    public long CategoryId { get; set; }
    
    // Navigation properties
    public Movie Movie { get; set; } = null!;
    public Category Category { get; set; } = null!;
}