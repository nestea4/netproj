using CatalogService.Data;
using CatalogService.Models;
using CatalogService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

/// <summary>
/// API для управління фільмами
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MovieController : ControllerBase
{
    private readonly CatalogDbContext _context;
    private readonly ILogger<MovieController> _logger;

    public MovieController(CatalogDbContext context, ILogger<MovieController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Отримання всіх фільмів
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MovieResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllMovies()
    {
        var movies = await _context.Movies
            .Include(m => m.Details)
            .Include(m => m.MovieCategories)
                .ThenInclude(mc => mc.Category)
            .Where(m => !m.IsDeleted)
            .Select(m => new MovieResponse
            {
                MovieId = m.MovieId,
                Title = m.Title,
                Description = m.Description,
                DurationMinutes = m.DurationMinutes,
                ReleaseDate = m.ReleaseDate,
                Director = m.Director,
                Rating = m.Rating,
                PosterUrl = m.PosterUrl,
                Categories = m.MovieCategories.Select(mc => mc.Category.Name).ToList(),
                Details = m.Details != null ? new MovieDetailsDto
                {
                    Country = m.Details.Country,
                    Language = m.Details.Language,
                    AgeRating = m.Details.AgeRating,
                    Cast = m.Details.Cast
                } : null
            })
            .ToListAsync();

        return Ok(movies);
    }

    /// <summary>
    /// Отримання фільму за ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMovieById(long id)
    {
        var movie = await _context.Movies
            .Include(m => m.Details)
            .Include(m => m.MovieCategories)
                .ThenInclude(mc => mc.Category)
            .Where(m => m.MovieId == id && !m.IsDeleted)
            .Select(m => new MovieResponse
            {
                MovieId = m.MovieId,
                Title = m.Title,
                Description = m.Description,
                DurationMinutes = m.DurationMinutes,
                ReleaseDate = m.ReleaseDate,
                Director = m.Director,
                Rating = m.Rating,
                PosterUrl = m.PosterUrl,
                Categories = m.MovieCategories.Select(mc => mc.Category.Name).ToList(),
                Details = m.Details != null ? new MovieDetailsDto
                {
                    Country = m.Details.Country,
                    Language = m.Details.Language,
                    AgeRating = m.Details.AgeRating,
                    Cast = m.Details.Cast
                } : null
            })
            .FirstOrDefaultAsync();

        if (movie == null)
            return NotFound(new ErrorResponse { Error = "Movie not found" });

        return Ok(movie);
    }

    /// <summary>
    /// Створення нового фільму
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMovie([FromBody] CreateMovieRequest request)
    {
        try
        {
            var movie = new Movie
            {
                Title = request.Title,
                OriginalTitle = request.OriginalTitle,
                Description = request.Description,
                DurationMinutes = request.DurationMinutes,
                ReleaseDate = request.ReleaseDate,
                Director = request.Director,
                Rating = request.Rating,
                PosterUrl = request.PosterUrl,
                CreatedBy = "API"
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            // Додавання категорій (M:N)
            if (request.CategoryIds.Any())
            {
                foreach (var categoryId in request.CategoryIds)
                {
                    _context.MovieCategories.Add(new MovieCategory
                    {
                        MovieId = movie.MovieId,
                        CategoryId = categoryId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(
                nameof(GetMovieById),
                new { id = movie.MovieId },
                new ApiResponse<long>
                {
                    Success = true,
                    Message = "Movie created successfully",
                    Data = movie.MovieId
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating movie");
            return BadRequest(new ErrorResponse
            {
                Error = "Failed to create movie",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Оновлення фільму
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMovie(long id, [FromBody] CreateMovieRequest request)
    {
        var movie = await _context.Movies.FindAsync(id);
        
        if (movie == null || movie.IsDeleted)
            return NotFound(new ErrorResponse { Error = "Movie not found" });

        movie.Title = request.Title;
        movie.OriginalTitle = request.OriginalTitle;
        movie.Description = request.Description;
        movie.DurationMinutes = request.DurationMinutes;
        movie.ReleaseDate = request.ReleaseDate;
        movie.Director = request.Director;
        movie.Rating = request.Rating;
        movie.PosterUrl = request.PosterUrl;
        movie.UpdatedBy = "API";

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Movie updated successfully",
            Data = movie.Title
        });
    }

    /// <summary>
    /// Видалення фільму (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMovie(long id)
    {
        var movie = await _context.Movies.FindAsync(id);
        
        if (movie == null || movie.IsDeleted)
            return NotFound(new ErrorResponse { Error = "Movie not found" });

        movie.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Movie deleted successfully",
            Data = movie.Title
        });
    }

    /// <summary>
    /// Пошук фільмів за назвою
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<MovieResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchMovies([FromQuery] string query)
    {
        var movies = await _context.Movies
            .Where(m => !m.IsDeleted && m.Title.Contains(query))
            .Select(m => new MovieResponse
            {
                MovieId = m.MovieId,
                Title = m.Title,
                Description = m.Description,
                DurationMinutes = m.DurationMinutes,
                Director = m.Director,
                Rating = m.Rating
            })
            .ToListAsync();

        return Ok(movies);
    }

    /// <summary>
    /// Фільми за категорією
    /// </summary>
    [HttpGet("category/{categoryId}")]
    [ProducesResponseType(typeof(IEnumerable<MovieResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMoviesByCategory(long categoryId)
    {
        var movies = await _context.MovieCategories
            .Where(mc => mc.CategoryId == categoryId)
            .Include(mc => mc.Movie)
            .Where(mc => !mc.Movie.IsDeleted)
            .Select(mc => new MovieResponse
            {
                MovieId = mc.Movie.MovieId,
                Title = mc.Movie.Title,
                Description = mc.Movie.Description,
                DurationMinutes = mc.Movie.DurationMinutes,
                Director = mc.Movie.Director,
                Rating = mc.Movie.Rating
            })
            .ToListAsync();

        return Ok(movies);
    }
}