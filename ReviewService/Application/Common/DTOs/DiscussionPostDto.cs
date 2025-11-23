namespace ReviewService.Application.Common.DTOs;

public class DiscussionPostDto
{
    public string PostId { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Likes { get; set; }
    public DateTime CreatedAt { get; set; }
}