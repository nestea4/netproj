using ReviewService.Domain.Entities;
using ReviewService.Domain.Entities.Review;

namespace ReviewService.Domain.Interfaces;

/// <summary>
/// Репозиторій для відгуків
/// </summary>
public interface IReviewRepository : IRepository<Review>
{
    Task<List<Review>> GetByMovieIdAsync(long movieId, CancellationToken cancellationToken = default);
    Task<List<Review>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<List<Review>> GetTopReviewsAsync(long movieId, int limit, CancellationToken cancellationToken = default);
    Task<double> GetAverageRatingAsync(long movieId, CancellationToken cancellationToken = default);
    Task<ReviewStatisticsDto> GetMovieStatisticsAsync(long movieId, CancellationToken cancellationToken = default);
    Task<List<Review>> SearchAsync(string query, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO для статистики (результат агрегації)
/// </summary>
public class ReviewStatisticsDto
{
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public int TotalLikes { get; set; }
    public int TotalComments { get; set; }
}