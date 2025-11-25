using System.Text.Json;
namespace AggregatorService.Clients;

public interface IReviewClient
{
    Task<List<ReviewDto>> GetMovieReviewsAsync(long movieId, CancellationToken ct = default);
    Task<ReviewStatisticsDto?> GetMovieStatisticsAsync(long movieId, CancellationToken ct = default);
}

public class ReviewClient : IReviewClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ReviewClient> _logger;

    public ReviewClient(HttpClient httpClient, ILogger<ReviewClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ReviewDto>> GetMovieReviewsAsync(long movieId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Calling ReviewService: GET /api/reviews/movie/{MovieId}", movieId);
            
            var response = await _httpClient.GetAsync($"/api/reviews/movie/{movieId}", ct);
            
            if (!response.IsSuccessStatusCode)
            {
                return new List<ReviewDto>();
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<ReviewDto>>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            }) ?? new List<ReviewDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting movie reviews from ReviewService");
            return new List<ReviewDto>();
        }
    }

    public async Task<ReviewStatisticsDto?> GetMovieStatisticsAsync(long movieId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Calling ReviewService: GET /api/reviews/movie/{MovieId}/statistics", movieId);
            
            var response = await _httpClient.GetAsync($"/api/reviews/movie/{movieId}/statistics", ct);
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<ReviewStatisticsDto>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting movie statistics from ReviewService");
            return null;
        }
    }
}
