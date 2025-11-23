namespace ReviewService.Application.Common.DTOs;

public class ReviewStatisticsDto
{
    public int Likes { get; set; }
    public int Dislikes { get; set; }
    public int ViewCount { get; set; }
    public int CommentCount { get; set; }
}