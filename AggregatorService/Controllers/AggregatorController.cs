using AggregatorService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AggregatorService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AggregatorController : ControllerBase
{
    private readonly IMovieAggregationService _aggregationService;
    private readonly ILogger<AggregatorController> _logger;

    public AggregatorController(
        IMovieAggregationService aggregationService,
        ILogger<AggregatorController> logger)
    {
        _aggregationService = aggregationService;
        _logger = logger;
    }

    /// <summary>
    ///отримання повної інформації про фільм (каталог + відгуки + статистика)
    /// </summary>
    [HttpGet("movie/{movieId}")]
    [ProducesResponseType(typeof(MovieDetailsAggregatedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMovieDetails(
        long movieId, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Aggregating movie details for MovieId: {MovieId}", movieId);

        var result = await _aggregationService.GetMovieDetailsAsync(movieId, cancellationToken);

        if (result == null)
        {
            return NotFound(new { Error = $"Movie {movieId} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            Service = "Aggregator Service",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }
}