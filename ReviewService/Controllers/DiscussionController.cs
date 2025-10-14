using Microsoft.AspNetCore.Mvc;
using ReviewService.Data.Repositories;
using ReviewService.Models;
using ReviewService.Models.DTOs;

namespace ReviewService.Controllers;

// <summary>
/// API для управління обговореннями
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DiscussionController : ControllerBase
{
    private readonly DiscussionRepository _discussionRepository;
    private readonly ILogger<DiscussionController> _logger;

    public DiscussionController(DiscussionRepository discussionRepository, ILogger<DiscussionController> logger)
    {
        _discussionRepository = discussionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Створення обговорення
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDiscussion([FromBody] CreateDiscussionRequest request)
    {
        try
        {
            var discussion = new Discussion
            {
                MovieId = request.MovieId,
                MovieTitle = request.MovieTitle,
                Topic = request.Topic,
                Description = request.Description,
                CreatedBy = request.CreatedBy,
                CreatedByName = request.CreatedByName
            };

            var created = await _discussionRepository.CreateAsync(discussion);

            return CreatedAtAction(
                nameof(GetDiscussionById),
                new { id = created.Id },
                new ApiResponse<string>
                {
                    Success = true,
                    Message = "Discussion created successfully",
                    Data = created.Id
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating discussion");
            return BadRequest(new ErrorResponse { Error = "Failed to create discussion", Details = ex.Message });
        }
    }

    /// <summary>
    /// Отримання обговорення за ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Discussion), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDiscussionById(string id)
    {
        var discussion = await _discussionRepository.GetByIdAsync(id);

        if (discussion == null)
            return NotFound(new ErrorResponse { Error = "Discussion not found" });

        // Збільшити лічильник переглядів
        await _discussionRepository.IncrementViewCountAsync(id);

        return Ok(discussion);
    }

    /// <summary>
    /// Обговорення фільму
    /// </summary>
    [HttpGet("movie/{movieId}")]
    [ProducesResponseType(typeof(IEnumerable<DiscussionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovieDiscussions(long movieId)
    {
        var discussions = await _discussionRepository.GetByMovieIdAsync(movieId);

        var response = discussions.Select(d => new DiscussionResponse
        {
            Id = d.Id,
            MovieTitle = d.MovieTitle,
            Topic = d.Topic,
            Description = d.Description,
            CreatedByName = d.CreatedByName,
            PostCount = d.Posts.Count,
            ParticipantCount = d.ParticipantCount,
            ViewCount = d.ViewCount,
            IsPinned = d.IsPinned,
            IsLocked = d.IsLocked,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        });

        return Ok(response);
    }

    /// <summary>
    /// Додавання поста в обговорення
    /// </summary>
    [HttpPost("{id}/posts")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddPost(string id, [FromBody] AddPostRequest request)
    {
        try
        {
            var post = new DiscussionPost
            {
                UserId = request.UserId,
                UserName = request.UserName,
                Content = request.Content
            };

            var success = await _discussionRepository.AddPostAsync(id, post);

            if (!success)
                return NotFound(new ErrorResponse { Error = "Discussion not found" });

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Post added successfully",
                Data = post.PostId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding post");
            return BadRequest(new ErrorResponse { Error = "Failed to add post", Details = ex.Message });
        }
    }

    /// <summary>
    /// Закріплення обговорення
    /// </summary>
    [HttpPatch("{id}/pin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PinDiscussion(string id, [FromQuery] bool isPinned)
    {
        var success = await _discussionRepository.PinDiscussionAsync(id, isPinned);

        if (!success)
            return NotFound(new ErrorResponse { Error = "Discussion not found" });

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = isPinned ? "Discussion pinned" : "Discussion unpinned",
            Data = id
        });
    }

    /// <summary>
    /// Блокування обговорення
    /// </summary>
    [HttpPatch("{id}/lock")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LockDiscussion(string id, [FromQuery] bool isLocked)
    {
        var success = await _discussionRepository.LockDiscussionAsync(id, isLocked);

        if (!success)
            return NotFound(new ErrorResponse { Error = "Discussion not found" });

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = isLocked ? "Discussion locked" : "Discussion unlocked",
            Data = id
        });
    }

    /// <summary>
    /// Популярні обговорення
    /// </summary>
    [HttpGet("popular")]
    [ProducesResponseType(typeof(IEnumerable<DiscussionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPopularDiscussions([FromQuery] int limit = 10)
    {
        var discussions = await _discussionRepository.GetPopularDiscussionsAsync(limit);

        var response = discussions.Select(d => new DiscussionResponse
        {
            Id = d.Id,
            MovieTitle = d.MovieTitle,
            Topic = d.Topic,
            PostCount = d.Posts.Count,
            ParticipantCount = d.ParticipantCount,
            ViewCount = d.ViewCount,
            CreatedAt = d.CreatedAt
        });

        return Ok(response);
    }

    /// <summary>
    /// Видалення обговорення
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteDiscussion(string id)
    {
        var success = await _discussionRepository.DeleteAsync(id);

        if (!success)
            return NotFound(new ErrorResponse { Error = "Discussion not found" });

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Discussion deleted successfully",
            Data = id
        });
    }
}