namespace CatalogService.Models.DTOs;

/// <summary>
/// Результат з пагінацією
/// </summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

public class MovieDto
{
    public long MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalTitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Director { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string PosterUrl { get; set; } = string.Empty;
    public string TrailerUrl { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = new();
}

public class MovieDetailDto
{
    public long MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalTitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Director { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string PosterUrl { get; set; } = string.Empty;
    public string TrailerUrl { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = new();
    
    // Додаткові деталі
    public MovieDetailsDto? Details { get; set; }
    public List<ShowtimeDto> UpcomingShowtimes { get; set; } = new();
}

public class MovieDetailsDto
{
    public string Country { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Budget { get; set; } = string.Empty;
    public string BoxOffice { get; set; } = string.Empty;
    public string Cast { get; set; } = string.Empty;
    public string AgeRating { get; set; } = string.Empty;
    public string Awards { get; set; } = string.Empty;
}

// ✅ Валідація тільки через FluentValidation
public class CreateMovieDto
{
    public string Title { get; set; } = string.Empty;
    public string? OriginalTitle { get; set; }
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? Director { get; set; }
    public decimal? Rating { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public List<long> CategoryIds { get; set; } = new();
    public CreateMovieDetailsDto? Details { get; set; }
}

public class CreateMovieDetailsDto
{
    public string? Country { get; set; }
    public string? Language { get; set; }
    public string? Budget { get; set; }
    public string? BoxOffice { get; set; }
    public string? Cast { get; set; }
    public string? AgeRating { get; set; }
    public string? Awards { get; set; }
}

public class UpdateMovieDto
{
    public string Title { get; set; } = string.Empty;
    public string? OriginalTitle { get; set; }
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? Director { get; set; }
    public decimal Rating { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public List<long> CategoryIds { get; set; } = new();
}

public class ShowtimeDto
{
    public long ShowtimeId { get; set; }
    public long MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public long HallId { get; set; }
    public string HallName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public int AvailableSeats { get; set; }
    public bool IsActive { get; set; }
}

public class CreateShowtimeDto
{
    public long MovieId { get; set; }
    public long HallId { get; set; }
    public DateTime StartTime { get; set; }
    public decimal BasePrice { get; set; }
}

public class CategoryDto
{
    public long CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int MovieCount { get; set; }
}

public class MovieFilterParameters
{
    public string? SearchTerm { get; set; }
    public long? CategoryId { get; set; }
    public decimal? MinRating { get; set; }
    public DateTime? ReleaseDateFrom { get; set; }
    public DateTime? ReleaseDateTo { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}