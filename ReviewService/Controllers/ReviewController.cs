using Microsoft.AspNetCore.Mvc;
using ReviewService.Data.Repositories;
using ReviewService.Models;
using ReviewService.Models.DTOs;

namespace ReviewService.Controllers;

/// <summary>
/// API для управління відгуками
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReviewController : ControllerBase
{
    private readonly ReviewRepository _reviewRepository;
    private readonly ILogger<ReviewController> _logger;

    public ReviewController(ReviewRepository reviewRepository, ILogger<ReviewController> logger)
    {
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    /// <summary>
    /// Створення нового відгуку
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        try
        {
            var review = new Review
            {
                MovieId = request.MovieId,
                MovieTitle = request.MovieTitle,
                UserId = request.UserId,
                UserName = request.UserName,
                Title = request.Title,
                Content = request.Content,
                Rating = request.Rating,
                ReviewType = request.ReviewType,
                IsVerifiedPurchase = request.IsVerifiedPurchase,
                IsSpoiler = request.IsSpoiler,
                Tags = request.Tags
            };

            var created = await _reviewRepository.CreateAsync(review);

            return CreatedAtAction(
                nameof(GetReviewById),
                new { id = created.Id },
                new ApiResponse<string>
                {
                    Success = true,
                    Message = "Review created successfully",
                    Data = created.Id
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review");
            return BadRequest(new ErrorResponse
            {
                Error = "Failed to create review",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Отримання відгуку за ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Review), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReviewById(string id)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        
        if (review == null)
            return NotFound(new ErrorResponse { Error = "Review not found" });

        //збільшити лічильник переглядів
        await _reviewRepository.IncrementViewCountAsync(id);

        return Ok(review);
    }

    /// <summary>
    /// Отримання всіх відгуків фільму
    /// </summary>
    [HttpGet("movie/{movieId}")]
    [ProducesResponseType(typeof(IEnumerable<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovieReviews(long movieId)
    {
        var reviews = await _reviewRepository.GetByMovieIdAsync(movieId);
        
        var response = reviews.Select(r => new ReviewResponse
        {
            Id = r.Id,
            MovieId = r.MovieId,
            MovieTitle = r.MovieTitle,
            UserName = r.UserName,
            Title = r.Title,
            Content = r.Content,
            Rating = r.Rating,
            ReviewType = r.ReviewType,
            Likes = r.Likes,
            Dislikes = r.Dislikes,
            ViewCount = r.ViewCount,
            CommentCount = r.Comments.Count,
            IsVerifiedPurchase = r.IsVerifiedPurchase,
            IsSpoiler = r.IsSpoiler,
            Tags = r.Tags,
            CreatedAt = r.CreatedAt
        });

        return Ok(response);
    }

    /// <summary>
    /// Топ відгуки фільму
    /// </summary>
    [HttpGet("movie/{movieId}/top")]
    [ProducesResponseType(typeof(IEnumerable<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopReviews(long movieId, [FromQuery] int limit = 10)
    {
        var reviews = await _reviewRepository.GetTopReviewsAsync(movieId, limit);
        
        var response = reviews.Select(r => new ReviewResponse
        {
            Id = r.Id,
            MovieId = r.MovieId,
            MovieTitle = r.MovieTitle,
            UserName = r.UserName,
            Title = r.Title,
            Content = r.Content,
            Rating = r.Rating,
            Likes = r.Likes,
            CommentCount = r.Comments.Count,
            CreatedAt = r.CreatedAt
        });

        return Ok(response);
    }

    /// <summary>
    /// Відгуки користувача
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserReviews(long userId)
    {
        var reviews = await _reviewRepository.GetByUserIdAsync(userId);
        
        var response = reviews.Select(r => new ReviewResponse
        {
            Id = r.Id,
            MovieId = r.MovieId,
            MovieTitle = r.MovieTitle,
            Title = r.Title,
            Rating = r.Rating,
            Likes = r.Likes,
            CommentCount = r.Comments.Count,
            CreatedAt = r.CreatedAt
        });

        return Ok(response);
    }

    /// <summary>
    /// Додавання коментаря до відгуку
    /// </summary>
    [HttpPost("{id}/comments")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddComment(string id, [FromBody] AddCommentRequest request)
    {
        try
        {
            var comment = new Comment
            {
                UserId = request.UserId,
                UserName = request.UserName,
                Content = request.Content
            };

            var success = await _reviewRepository.AddCommentAsync(id, comment);

            if (!success)
                return NotFound(new ErrorResponse { Error = "Review not found" });

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Comment added successfully",
                Data = comment.CommentId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment");
            return BadRequest(new ErrorResponse { Error = "Failed to add comment", Details = ex.Message });
        }
    }

    /// <summary>
    /// Лайк/дизлайк відгуку
    /// </summary>
    [HttpPost("{id}/like")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LikeReview(string id, [FromBody] LikeRequest request)
    {
        var success = await _reviewRepository.UpdateLikesAsync(id, request.IsLike);

        if (!success)
            return NotFound(new ErrorResponse { Error = "Review not found" });

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = request.IsLike ? "Review liked" : "Review disliked",
            Data = id
        });
    }

    /// <summary>
    /// Статистика відгуків по фільму (aggregate)
    /// </summary>
    [HttpGet("movie/{movieId}/statistics")]
    [ProducesResponseType(typeof(ReviewStatistics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovieStatistics(long movieId)
    {
        var stats = await _reviewRepository.GetMovieStatisticsAsync(movieId);
        return Ok(stats);
    }

    /// <summary>
    /// Середній рейтинг фільму
    /// </summary>
    [HttpGet("movie/{movieId}/rating")]
    [ProducesResponseType(typeof(ApiResponse<double>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAverageRating(long movieId)
    {
        var avgRating = await _reviewRepository.GetAverageRatingAsync(movieId);
        return Ok(new ApiResponse<double>
        {
            Success = true,
            Message = "Average rating retrieved",
            Data = Math.Round(avgRating, 1)
        });
    }

    /// <summary>
    /// Пошук відгуків
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchReviews([FromQuery] string query)
    {
        var reviews = await _reviewRepository.SearchAsync(query);
        
        var response = reviews.Select(r => new ReviewResponse
        {
            Id = r.Id,
            MovieTitle = r.MovieTitle,
            UserName = r.UserName,
            Title = r.Title,
            Content = r.Content.Length > 200 ? r.Content.Substring(0, 200) + "..." : r.Content,
            Rating = r.Rating,
            Likes = r.Likes,
            CreatedAt = r.CreatedAt
        });

        return Ok(response);
    }

    /// <summary>
    /// Видалення відгуку
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteReview(string id)
    {
        var success = await _reviewRepository.DeleteAsync(id);

        if (!success)
            return NotFound(new ErrorResponse { Error = "Review not found" });

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Review deleted successfully",
            Data = id
        });
    }
}