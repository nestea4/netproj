using CatalogService.Data;
using CatalogService.Models;
using CatalogService.Models.DTOs;
using CatalogService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

/// <summary>
/// API для управління сеансами
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ShowtimesController : ControllerBase
{
    private readonly IShowtimeService _showtimeService;
    private readonly ILogger<ShowtimesController> _logger;

    public ShowtimesController(IShowtimeService showtimeService, ILogger<ShowtimesController> logger)
    {
        _showtimeService = showtimeService;
        _logger = logger;
    }

    /// <summary>
    /// Отримання списку сеансів з фільтрацією та пагінацією
    /// </summary>
    /// <param name="movieId">ID фільму (опціонально)</param>
    /// <param name="date">Дата сеансу (опціонально)</param>
    /// <param name="pageNumber">Номер сторінки</param>
    /// <param name="pageSize">Розмір сторінки</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Посторінковий список сеансів</returns>
    /// <response code="200">Успішне отримання списку сеансів</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ShowtimeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShowtimes(
        [FromQuery] long? movieId,
        [FromQuery] DateTime? date,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _showtimeService.GetShowtimesAsync(
            movieId, date, pageNumber, pageSize, cancellationToken);

        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        Response.Headers.Append("X-Total-Pages", result.TotalPages.ToString());

        return Ok(result);
    }

    /// <summary>
    /// Отримання сеансу за ID
    /// </summary>
    /// <param name="id">ID сеансу</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Інформація про сеанс</returns>
    /// <response code="200">Сеанс знайдено</response>
    /// <response code="404">Сеанс не знайдено</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ShowtimeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShowtimeById(long id, CancellationToken cancellationToken)
    {
        var showtime = await _showtimeService.GetShowtimeByIdAsync(id, cancellationToken);
        return Ok(showtime);
    }

    /// <summary>
    /// Створення нового сеансу
    /// </summary>
    /// <param name="dto">Дані нового сеансу</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Створений сеанс</returns>
    /// <response code="201">Сеанс успішно створено</response>
    /// <response code="400">Некоректні дані</response>
    /// <response code="404">Фільм або зал не знайдено</response>
    /// <response code="409">Зал зайнятий у вказаний час</response>
    [HttpPost]
    [ProducesResponseType(typeof(ShowtimeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateShowtime(
        [FromBody] CreateShowtimeDto dto,
        CancellationToken cancellationToken)
    {
        var showtime = await _showtimeService.CreateShowtimeAsync(dto, cancellationToken);
        
        return CreatedAtAction(
            nameof(GetShowtimeById),
            new { id = showtime.ShowtimeId },
            showtime);
    }

    /// <summary>
    /// Видалення сеансу
    /// </summary>
    /// <param name="id">ID сеансу</param>
    /// <param name="cancellationToken"></param>
    /// <response code="204">Сеанс успішно видалено</response>
    /// <response code="404">Сеанс не знайдено</response>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteShowtime(long id, CancellationToken cancellationToken)
    {
        await _showtimeService.DeleteShowtimeAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Оновлення кількості доступних місць (для Booking Service)
    /// </summary>
    /// <param name="id">ID сеансу</param>
    /// <param name="seatsToReserve">Кількість місць для резервування</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Результат операції</returns>
    /// <response code="200">Місця успішно зарезервовано</response>
    /// <response code="400">Недостатньо вільних місць</response>
    /// <response code="404">Сеанс не знайдено</response>
    [HttpPatch("{id:long}/seats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSeats(
        long id,
        [FromBody] int seatsToReserve,
        CancellationToken cancellationToken)
    {
        await _showtimeService.UpdateSeatsAsync(id, seatsToReserve, cancellationToken);
        
        return Ok(new
        {
            Success = true,
            Message = "Seats reserved successfully",
            ShowtimeId = id,
            ReservedSeats = seatsToReserve
        });
    }
}