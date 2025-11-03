using CatalogService.Data;
using CatalogService.Models;
using CatalogService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

/// <summary>
/// API для управління сеансами
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ShowtimeController : ControllerBase
{
    private readonly CatalogDbContext _context;
    private readonly ILogger<ShowtimeController> _logger;

    public ShowtimeController(CatalogDbContext context, ILogger<ShowtimeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Отримання всіх сеансів
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ShowtimeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllShowtimes()
    {
        var showtimes = await _context.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => !s.IsDeleted && s.IsActive)
            .Select(s => new ShowtimeResponse
            {
                ShowtimeId = s.ShowtimeId,
                MovieTitle = s.Movie.Title,
                HallName = s.Hall.Name,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                BasePrice = s.BasePrice,
                AvailableSeats = s.AvailableSeats,
                IsActive = s.IsActive
            })
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        return Ok(showtimes);
    }

    /// <summary>
    /// Отримання сеансу за ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ShowtimeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShowtimeById(long id)
    {
        var showtime = await _context.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.ShowtimeId == id && !s.IsDeleted)
            .Select(s => new ShowtimeResponse
            {
                ShowtimeId = s.ShowtimeId,
                MovieTitle = s.Movie.Title,
                HallName = s.Hall.Name,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                BasePrice = s.BasePrice,
                AvailableSeats = s.AvailableSeats,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync();

        if (showtime == null)
            return NotFound(new ErrorResponse { Error = "Showtime not found" });

        return Ok(showtime);
    }

    /// <summary>
    /// Створення нового сеансу
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateShowtime([FromBody] CreateShowtimeRequest request)
    {
        try
        {
            //перевірка існування фільму та залу
            var movie = await _context.Movies.FindAsync(request.MovieId);
            var hall = await _context.Halls.FindAsync(request.HallId);

            if (movie == null || movie.IsDeleted)
                return BadRequest(new ErrorResponse { Error = "Movie not found" });

            if (hall == null || hall.IsDeleted)
                return BadRequest(new ErrorResponse { Error = "Hall not found" });

            //розрахунок EndTime на основі тривалості фільму
            var endTime = request.StartTime.AddMinutes(movie.DurationMinutes);

            var showtime = new Showtime
            {
                MovieId = request.MovieId,
                HallId = request.HallId,
                StartTime = request.StartTime,
                EndTime = endTime,
                BasePrice = request.BasePrice,
                AvailableSeats = hall.Capacity,
                IsActive = true
            };

            _context.Showtimes.Add(showtime);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetShowtimeById),
                new { id = showtime.ShowtimeId },
                new ApiResponse<long>
                {
                    Success = true,
                    Message = "Showtime created successfully",
                    Data = showtime.ShowtimeId
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating showtime");
            return BadRequest(new ErrorResponse
            {
                Error = "Failed to create showtime",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Сеанси для конкретного фільму
    /// </summary>
    [HttpGet("movie/{movieId}")]
    [ProducesResponseType(typeof(IEnumerable<ShowtimeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShowtimesByMovie(long movieId)
    {
        var showtimes = await _context.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.MovieId == movieId && !s.IsDeleted && s.IsActive)
            .Select(s => new ShowtimeResponse
            {
                ShowtimeId = s.ShowtimeId,
                MovieTitle = s.Movie.Title,
                HallName = s.Hall.Name,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                BasePrice = s.BasePrice,
                AvailableSeats = s.AvailableSeats,
                IsActive = s.IsActive
            })
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        return Ok(showtimes);
    }

    /// <summary>
    /// Сеанси на конкретну дату
    /// </summary>
    [HttpGet("date/{date}")]
    [ProducesResponseType(typeof(IEnumerable<ShowtimeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShowtimesByDate(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1);

        var showtimes = await _context.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.StartTime >= startOfDay && s.StartTime < endOfDay 
                     && !s.IsDeleted && s.IsActive)
            .Select(s => new ShowtimeResponse
            {
                ShowtimeId = s.ShowtimeId,
                MovieTitle = s.Movie.Title,
                HallName = s.Hall.Name,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                BasePrice = s.BasePrice,
                AvailableSeats = s.AvailableSeats,
                IsActive = s.IsActive
            })
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        return Ok(showtimes);
    }

    /// <summary>
    /// Оновлення доступних місць (для Booking Service)
    /// </summary>
    [HttpPatch("{id}/seats")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAvailableSeats(long id, [FromBody] int seatsToReserve)
    {
        var showtime = await _context.Showtimes.FindAsync(id);

        if (showtime == null || showtime.IsDeleted)
            return NotFound(new ErrorResponse { Error = "Showtime not found" });

        if (showtime.AvailableSeats < seatsToReserve)
            return BadRequest(new ErrorResponse { Error = "Not enough seats available" });

        showtime.AvailableSeats -= seatsToReserve;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<int>
        {
            Success = true,
            Message = "Seats reserved successfully",
            Data = showtime.AvailableSeats
        });
    }

    /// <summary>
    /// Видалення сеансу 
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteShowtime(long id)
    {
        var showtime = await _context.Showtimes
            .Include(s => s.Movie)
            .FirstOrDefaultAsync(s => s.ShowtimeId == id);

        if (showtime == null || showtime.IsDeleted)
            return NotFound(new ErrorResponse { Error = "Showtime not found" });

        showtime.IsDeleted = true;
        showtime.IsActive = false;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Showtime cancelled successfully",
            Data = showtime.Movie.Title
        });
    }
}