using System.Text.Json;

namespace AggregatorService.Clients;

public interface IBookingClient
{
    Task<BookingDto?> GetBookingAsync(long id, CancellationToken ct = default);
    Task<List<BookingDto>> GetCustomerBookingsAsync(long customerId, CancellationToken ct = default);
}

public class BookingClient : IBookingClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BookingClient> _logger;

    public BookingClient(HttpClient httpClient, ILogger<BookingClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<BookingDto?> GetBookingAsync(long id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Calling BookingService: GET /api/booking/{BookingId}", id);
            
            var response = await _httpClient.GetAsync($"/api/booking/{id}", ct);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BookingService returned {StatusCode} for booking {BookingId}", 
                    response.StatusCode, id);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<BookingDto>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling BookingService for booking {BookingId}", id);
            return null;
        }
    }

    public async Task<List<BookingDto>> GetCustomerBookingsAsync(long customerId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Calling BookingService: GET /api/booking/customer/{CustomerId}", customerId);
            
            var response = await _httpClient.GetAsync($"/api/booking/customer/{customerId}", ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<BookingDto>>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            }) ?? new List<BookingDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer bookings from BookingService");
            return new List<BookingDto>();
        }
    }
}


// DTOs
public record BookingDto(long BookingId, string BookingNumber, string Status);
public record MovieDto(long MovieId, string Title, string? Description);
public record ShowtimeDto(long ShowtimeId, DateTime ShowDateTime, string HallName);
public record ReviewDto(string ReviewId, long MovieId, int Rating, string Content);
public record ReviewStatisticsDto(long MovieId, double AverageRating, int TotalReviews);