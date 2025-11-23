using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReviewService.Application.Commands.Reviews;
using ReviewService.Application.Common.DTOs;
using ReviewService.Application.Queries.Reviews;

namespace ReviewService.WebAPI.Controllers;

/// <summary>
///для управління відгуками
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    ///створення нового відгуку
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateReviewResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReview(
        [FromBody] CreateReviewCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetReviewById),
            new { id = result.ReviewId },
            result);
    }

    /// <summary>
    ///отримання відгуку за ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReviewById(
        string id,
        CancellationToken cancellationToken)
    {
        var query = new GetReviewByIdQuery { ReviewId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    ///отримання всіх відгуків фільму
    /// </summary>
    [HttpGet("movie/{movieId}")]
    [ProducesResponseType(typeof(List<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovieReviews(
        long movieId,
        CancellationToken cancellationToken)
    {
        var query = new GetMovieReviewsQuery { MovieId = movieId };
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    ///оновлення відгуку
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReview(
        string id,
        [FromBody] UpdateReviewCommand command,
        CancellationToken cancellationToken)
    {
        //ID з route співпадає з ID в body
        var commandWithId = command with { ReviewId = id };
        
        await _mediator.Send(commandWithId, cancellationToken);

        return NoContent();
    }

    /// <summary>
    ///lодавання коментаря до відгуку
    /// </summary>
    [HttpPost("{id}/comments")]
    [ProducesResponseType(typeof(AddCommentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment(
        string id,
        [FromBody] AddCommentCommand command,
        CancellationToken cancellationToken)
    {
        var commandWithId = command with { ReviewId = id };
        var result = await _mediator.Send(commandWithId, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    ///лайк/дизлайк відгуку
    /// </summary>
    [HttpPost("{id}/like")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LikeReview(
        string id,
        [FromQuery] bool isLike,
        CancellationToken cancellationToken)
    {
        var command = new LikeReviewCommand { ReviewId = id, IsLike = isLike };
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    ///статистика відгуків фільму
    /// </summary>
    [HttpGet("movie/{movieId}/statistics")]
    [ProducesResponseType(typeof(ReviewStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovieStatistics(
        long movieId,
        CancellationToken cancellationToken)
    {
        var query = new GetMovieStatisticsQuery { MovieId = movieId };
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    ///видалення відгуку
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview(
        string id,
        [FromQuery] long userId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteReviewCommand { ReviewId = id, UserId = userId };
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
}