namespace CatalogService.Models;

/// <summary>
/// Категорія/жанр фільму
/// </summary>
public class Category
{
    public long CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; //URL-friendly: action, drama
    
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    //Navigation
    public ICollection<MovieCategory> MovieCategories { get; set; } = new List<MovieCategory>();
}