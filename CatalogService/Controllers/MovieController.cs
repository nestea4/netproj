using CatalogService.Data;
using CatalogService.Models;
using CatalogService.Models.DTOs;
using CatalogService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

/// <summary>
/// API для управління фільмами
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly ILogger<MoviesController> _logger;

    public MoviesController(IMovieService movieService, ILogger<MoviesController> logger)
    {
        _movieService = movieService;
        _logger = logger;
    }

    /// <summary>
    /// Отримання списку фільмів з фільтрацією, сортуванням та пагінацією
    /// </summary>
    /// <param name="filters">Параметри фільтрації</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Посторінковий список фільмів</returns>
    /// <response code="200">Успішне отримання списку фільмів</response>
    /// <response code="400">Некоректні параметри запиту</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MovieDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMovies(
        [FromQuery] MovieFilterParameters filters,
        CancellationToken cancellationToken)
    {
        var result = await _movieService.GetMoviesAsync(filters, cancellationToken);
        
        //Link headers для пагінації
        if (result.HasNext)
        {
            Response.Headers.Append("X-Has-Next", "true");
        }
        if (result.HasPrevious)
        {
            Response.Headers.Append("X-Has-Previous", "true");
        }
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        Response.Headers.Append("X-Total-Pages", result.TotalPages.ToString());

        return Ok(result);
    }

    /// <summary>
    /// Отримання детальної інформації про фільм за ID
    /// </summary>
    /// <param name="id">ID фільму</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Детальна інформація про фільм</returns>
    /// <response code="200">Фільм знайдено</response>
    /// <response code="404">Фільм не знайдено</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(MovieDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMovieById(long id, CancellationToken cancellationToken)
    {
        var movie = await _movieService.GetMovieByIdAsync(id, cancellationToken);
        return Ok(movie);
    }

    /// <summary>
    /// Отримання популярних фільмів
    /// </summary>
    /// <param name="count">Кількість фільмів (за замовчуванням 10)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Список популярних фільмів</returns>
    /// <response code="200">Успішне отримання списку</response>
    [HttpGet("popular")]
    [ProducesResponseType(typeof(IEnumerable<MovieDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPopularMovies(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var movies = await _movieService.GetPopularMoviesAsync(count, cancellationToken);
        return Ok(movies);
    }

    /// <summary>
    /// Створення нового фільму
    /// </summary>
    /// <param name="dto">Дані нового фільму</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Створений фільм</returns>
    /// <response code="201">Фільм успішно створено</response>
    /// <response code="400">Некоректні дані</response>
    [HttpPost]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMovie(
        [FromBody] CreateMovieDto dto,
        CancellationToken cancellationToken)
    {
        var movie = await _movieService.CreateMovieAsync(dto, cancellationToken);
        
        return CreatedAtAction(
            nameof(GetMovieById),
            new { id = movie.MovieId },
            movie);
    }

    /// <summary>
    /// Оновлення інформації про фільм
    /// </summary>
    /// <param name="id">ID фільму</param>
    /// <param name="dto">Оновлені дані фільму</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Оновлений фільм</returns>
    /// <response code="200">Фільм успішно оновлено</response>
    /// <response code="400">Некоректні дані</response>
    /// <response code="404">Фільм не знайдено</response>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMovie(
        long id,
        [FromBody] UpdateMovieDto dto,
        CancellationToken cancellationToken)
    {
        var movie = await _movieService.UpdateMovieAsync(id, dto, cancellationToken);
        return Ok(movie);
    }

    /// <summary>
    /// Видалення фільму (soft delete)
    /// </summary>
    /// <param name="id">ID фільму</param>
    /// <param name="cancellationToken"></param>
    /// <response code="204">Фільм успішно видалено</response>
    /// <response code="404">Фільм не знайдено</response>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMovie(long id, CancellationToken cancellationToken)
    {
        await _movieService.DeleteMovieAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Отримання фільмів за категорією
    /// </summary>
    /// <param name="categoryId">ID категорії</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Список фільмів категорії</returns>
    /// <response code="200">Успішне отримання списку</response>
    /// <response code="404">Категорія не знайдена</response>
    [HttpGet("category/{categoryId:long}")]
    [ProducesResponseType(typeof(IEnumerable<MovieDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMoviesByCategory(
        long categoryId,
        CancellationToken cancellationToken)
    {
        var movies = await _movieService.GetMoviesByCategoryAsync(categoryId, cancellationToken);
        return Ok(movies);
    }
}