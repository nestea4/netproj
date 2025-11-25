using AggregatorService.Clients;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AggregatorService.Services;

public interface IMovieAggregationService
{
    Task<MovieDetailsAggregatedDto?> GetMovieDetailsAsync(long movieId, CancellationToken ct = default);
}

public class MovieAggregationService : IMovieAggregationService
{
    private readonly ICatalogClient _catalogClient;
    private readonly IReviewClient _reviewClient;
    private readonly ILogger<MovieAggregationService> _logger;

    public MovieAggregationService(
        ICatalogClient catalogClient,
        IReviewClient reviewClient,
        ILogger<MovieAggregationService> logger)
    {
        _catalogClient = catalogClient;
        _reviewClient = reviewClient;
        _logger = logger;
    }

    public async Task<MovieDetailsAggregatedDto?> GetMovieDetailsAsync(long movieId, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting aggregation for movie {MovieId}", movieId);

        try
        {
            //паралельні до різних сервісів
            var movieTask = _catalogClient.GetMovieAsync(movieId, ct);
            var reviewsTask = _reviewClient.GetMovieReviewsAsync(movieId, ct);
            var statisticsTask = _reviewClient.GetMovieStatisticsAsync(movieId, ct);

            //чекаємо всі разом (економимо час!)
            await Task.WhenAll(movieTask, reviewsTask, statisticsTask);

            stopwatch.Stop();
            _logger.LogInformation(
                "Aggregation completed for movie {MovieId} in {ElapsedMs}ms",
                movieId, 
                stopwatch.ElapsedMilliseconds);

            var movie = await movieTask;
            if (movie == null)
            {
                _logger.LogWarning("Movie {MovieId} not found in CatalogService", movieId);
                return null;
            }

            var reviews = await reviewsTask;
            var statistics = await statisticsTask;

            return new MovieDetailsAggregatedDto
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Description = movie.Description,
                Reviews = reviews,
                Statistics = statistics,
                AggregatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating data for movie {MovieId}", movieId);
            return null;
        }
    }
}

// AGGREGATED DTOs
public class MovieDetailsAggregatedDto
{
    public long MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();
    public ReviewStatisticsDto? Statistics { get; set; }
    public DateTime AggregatedAt { get; set; }
}

