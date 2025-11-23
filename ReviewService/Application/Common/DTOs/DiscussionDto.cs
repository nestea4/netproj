namespace ReviewService.Application.Common.DTOs;

public class DiscussionDto
{
    public string Id { get; set; } = string.Empty;
    public long MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public long CreatorId { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<DiscussionPostDto> Posts { get; set; } = new();
    public int ParticipantCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsPinned { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}