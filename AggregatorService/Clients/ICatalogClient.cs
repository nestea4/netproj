using System.Text.Json;
namespace AggregatorService.Clients;

public interface ICatalogClient
{
    Task<MovieDto?> GetMovieAsync(long id, CancellationToken ct = default);
    Task<ShowtimeDto?> GetShowtimeAsync(long id, CancellationToken ct = default);
}

public class CatalogClient : ICatalogClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogClient> _logger;

    public CatalogClient(HttpClient httpClient, ILogger<CatalogClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<MovieDto?> GetMovieAsync(long id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Calling CatalogService: GET /api/movies/{MovieId}", id);
            
            var response = await _httpClient.GetAsync($"/api/movies/{id}", ct);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CatalogService returned {StatusCode} for movie {MovieId}", 
                    response.StatusCode, id);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<MovieDto>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling CatalogService for movie {MovieId}", id);
            return null;
        }
    }

    public async Task<ShowtimeDto?> GetShowtimeAsync(long id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Calling CatalogService: GET /api/showtimes/{ShowtimeId}", id);
            
            var response = await _httpClient.GetAsync($"/api/showtimes/{id}", ct);
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<ShowtimeDto>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling CatalogService for showtime {ShowtimeId}", id);
            return null;
        }
    }
}
