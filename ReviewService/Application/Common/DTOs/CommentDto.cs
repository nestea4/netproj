namespace ReviewService.Application.Common.DTOs;

public class CommentDto
{
    public string CommentId { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Likes { get; set; }
    public List<CommentReplyDto> Replies { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}