namespace ReviewService.Application.Common.DTOs;

public class CommentReplyDto
{
    public string ReplyId { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}