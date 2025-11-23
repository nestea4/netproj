using ReviewService.Domain.Enums;

namespace ReviewService.Application.Common.DTOs;

public class ReviewDto
{
    public string Id { get; set; } = string.Empty;
    public long MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
    public ReviewType ReviewType { get; set; }
    public bool IsSpoiler { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public List<string> Tags { get; set; } = new();
    public ReviewStatisticsDto Statistics { get; set; } = new();
    public List<CommentDto> Comments { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}