using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReviewService.Application.Commands.Discussions;
using ReviewService.Application.Common.DTOs;
using ReviewService.Application.Queries.Discussions;

namespace ReviewService.WebAPI.Controllers;

// <summary>
///для управління обговореннями
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DiscussionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DiscussionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    ///створення обговорення
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateDiscussionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDiscussion(
        [FromBody] CreateDiscussionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetDiscussionById),
            new { id = result.DiscussionId },
            result);
    }

    /// <summary>
    ///отримання обговорення за ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DiscussionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDiscussionById(
        string id,
        CancellationToken cancellationToken)
    {
        var query = new GetDiscussionByIdQuery { DiscussionId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    ///отримання обговорень фільму
    /// </summary>
    [HttpGet("movie/{movieId}")]
    [ProducesResponseType(typeof(List<DiscussionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovieDiscussions(
        long movieId,
        CancellationToken cancellationToken)
    {
        var query = new GetMovieDiscussionsQuery { MovieId = movieId };
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    ///додавання поста в обговорення
    /// </summary>
    [HttpPost("{id}/posts")]
    [ProducesResponseType(typeof(AddPostResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddPost(
        string id,
        [FromBody] AddPostCommand command,
        CancellationToken cancellationToken)
    {
        var commandWithId = command with { DiscussionId = id };
        var result = await _mediator.Send(commandWithId, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    ///популярні обговорення
    /// </summary>
    [HttpGet("popular")]
    [ProducesResponseType(typeof(List<DiscussionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPopularDiscussions(CancellationToken cancellationToken,
        [FromQuery] int limit = 10)
    {
        var query = new GetPopularDiscussionsQuery { Limit = limit };
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}