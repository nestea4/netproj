namespace CatalogService.Models.DTOs;

/// <summary>
/// Запит на створення фільму
/// </summary>
public class CreateMovieRequest
{
    public string Title { get; set; } = string.Empty;
    public string OriginalTitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Director { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string PosterUrl { get; set; } = string.Empty;
    public List<long> CategoryIds { get; set; } = new();
}

/// <summary>
/// Відповідь з інформацією про фільм
/// </summary>
public class MovieResponse
{
    public long MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Director { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string PosterUrl { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = new();
    public MovieDetailsDto? Details { get; set; }
}

/// <summary>
/// DTO для додаткових деталей фільму
/// </summary>
/*public class MovieDetailsDto
{
    public string Country { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string AgeRating { get; set; } = string.Empty;
    public string Cast { get; set; } = string.Empty;
}*/

/// <summary>
/// Запит на створення сеансу
/// </summary>
public class CreateShowtimeRequest
{
    public long MovieId { get; set; }
    public long HallId { get; set; }
    public DateTime StartTime { get; set; }
    public decimal BasePrice { get; set; }
}

/// <summary>
/// Відповідь з інформацією про сеанс
/// </summary>
public class ShowtimeResponse
{
    public long ShowtimeId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public int AvailableSeats { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Створення категорії
/// </summary>
public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Створення залу
/// </summary>
public class CreateHallRequest
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int RowCount { get; set; }
    public int SeatsPerRow { get; set; }
    public string HallType { get; set; } = "Standard";
}

/// <summary>
/// Стандартна API відповідь
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

/// <summary>
/// Відповідь з помилкою
/// </summary>
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}