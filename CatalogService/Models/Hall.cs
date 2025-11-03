namespace CatalogService.Models;

/// <summary>
/// Кінозал
/// </summary>
public class Hall
{
    public long HallId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int RowCount { get; set; }
    public int SeatsPerRow { get; set; }
    public string HallType { get; set; } = "Standard"; //Standard, IMAX, 3D, VIP
    public bool HasWheelchairAccess { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    //Navigation
    public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}