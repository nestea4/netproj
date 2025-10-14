namespace ReviewService.Models.DTOs;

/// <summary>
/// Запит на створення відгуку
/// </summary>
public class CreateReviewRequest
{
    public long MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-10
    public string ReviewType { get; set; } = "Standard";
    public bool IsVerifiedPurchase { get; set; }
    public bool IsSpoiler { get; set; }
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Відповідь з відгуком
/// </summary>
public class ReviewResponse
{
    public string Id { get; set; } = string.Empty;
    public long MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string ReviewType { get; set; } = string.Empty;
    public int Likes { get; set; }
    public int Dislikes { get; set; }
    public int ViewCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public bool IsSpoiler { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Додавання коментаря
/// </summary>
public class AddCommentRequest
{
    public string ReviewId { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Створення обговорення
/// </summary>
public class CreateDiscussionRequest
{
    public long MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
}

/// <summary>
/// Відповідь обговорення
/// </summary>
public class DiscussionResponse
{
    public string Id { get; set; } = string.Empty;
    public string MovieTitle { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public int ParticipantCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsPinned { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Додавання поста в обговорення
/// </summary>
public class AddPostRequest
{
    public string DiscussionId { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Лайк/дизлайк
/// </summary>
public class LikeRequest
{
    public long UserId { get; set; }
    public string TargetType { get; set; } = string.Empty; // "Review", "Comment"
    public string TargetId { get; set; } = string.Empty;
    public bool IsLike { get; set; } // true = like, false = dislike
}

/// <summary>
/// Стандартна API відповідь
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

/// <summary>
/// Відповідь з помилкою
/// </summary>
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}