using MongoDB.Driver;
using ReviewService.Models;

namespace ReviewService.Data.Repositories;

/// <summary>
/// Репозиторій для роботи з відгуками
/// </summary>
public class ReviewRepository
{
    private readonly IMongoCollection<Review> _reviews;

    public ReviewRepository(MongoDbContext context)
    {
        _reviews = context.Reviews;
    }

    /// <summary>
    /// Створення нового відгуку
    /// </summary>
    public async Task<Review> CreateAsync(Review review)
    {
        await _reviews.InsertOneAsync(review);
        return review;
    }

    /// <summary>
    /// Отримання відгуку за ID
    /// </summary>
    public async Task<Review?> GetByIdAsync(string id)
    {
        return await _reviews.Find(r => r.Id == id && !r.IsDeleted).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Отримання всіх відгуків фільму
    /// </summary>
    public async Task<List<Review>> GetByMovieIdAsync(long movieId)
    {
        return await _reviews
            .Find(r => r.MovieId == movieId && !r.IsDeleted)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Отримання відгуків користувача
    /// </summary>
    public async Task<List<Review>> GetByUserIdAsync(long userId)
    {
        return await _reviews
            .Find(r => r.UserId == userId && !r.IsDeleted)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Топ відгуки (за рейтингом та лайками)
    /// </summary>
    public async Task<List<Review>> GetTopReviewsAsync(long movieId, int limit = 10)
    {
        return await _reviews
            .Find(r => r.MovieId == movieId && !r.IsDeleted)
            .SortByDescending(r => r.Likes)
            .ThenByDescending(r => r.Rating)
            .Limit(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Оновлення відгуку
    /// </summary>
    public async Task<bool> UpdateAsync(string id, Review review)
    {
        review.UpdatedAt = DateTime.UtcNow;
        var result = await _reviews.ReplaceOneAsync(r => r.Id == id, review);
        return result.ModifiedCount > 0;
    }

    /// <summary>
    /// Додавання коментаря до відгуку
    /// </summary>
    public async Task<bool> AddCommentAsync(string reviewId, Comment comment)
    {
        var update = Builders<Review>.Update
            .Push(r => r.Comments, comment)
            .Set(r => r.UpdatedAt, DateTime.UtcNow);

        var result = await _reviews.UpdateOneAsync(r => r.Id == reviewId, update);
        return result.ModifiedCount > 0;
    }

    /// <summary>
    /// Додавання відповіді на коментар
    /// </summary>
    public async Task<bool> AddReplyToCommentAsync(string reviewId, string commentId, CommentReply reply)
    {
        var filter = Builders<Review>.Filter.And(
            Builders<Review>.Filter.Eq(r => r.Id, reviewId),
            Builders<Review>.Filter.ElemMatch(r => r.Comments, c => c.CommentId == commentId)
        );

        var update = Builders<Review>.Update
            .Push("comments.$.replies", reply)
            .Set(r => r.UpdatedAt, DateTime.UtcNow);

        var result = await _reviews.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    /// <summary>
    /// Оновлення лайків
    /// </summary>
    public async Task<bool> UpdateLikesAsync(string reviewId, bool isLike)
    {
        var update = isLike
            ? Builders<Review>.Update.Inc(r => r.Likes, 1)
            : Builders<Review>.Update.Inc(r => r.Dislikes, 1);

        var result = await _reviews.UpdateOneAsync(r => r.Id == reviewId, update);
        return result.ModifiedCount > 0;
    }

    /// <summary>
    /// Збільшення лічильника переглядів
    /// </summary>
    public async Task<bool> IncrementViewCountAsync(string reviewId)
    {
        var update = Builders<Review>.Update.Inc(r => r.ViewCount, 1);
        var result = await _reviews.UpdateOneAsync(r => r.Id == reviewId, update);
        return result.ModifiedCount > 0;
    }

    /// <summary>
    /// Видалення відгуку (soft delete)
    /// </summary>
    public async Task<bool> DeleteAsync(string id)
    {
        var update = Builders<Review>.Update.Set(r => r.IsDeleted, true);
        var result = await _reviews.UpdateOneAsync(r => r.Id == id, update);
        return result.ModifiedCount > 0;
    }

    /// <summary>
    /// Пошук відгуків (full-text search simulation)
    /// </summary>
    public async Task<List<Review>> SearchAsync(string query)
    {
        var filter = Builders<Review>.Filter.And(
            Builders<Review>.Filter.Eq(r => r.IsDeleted, false),
            Builders<Review>.Filter.Or(
                Builders<Review>.Filter.Regex(r => r.Title, new MongoDB.Bson.BsonRegularExpression(query, "i")),
                Builders<Review>.Filter.Regex(r => r.Content, new MongoDB.Bson.BsonRegularExpression(query, "i"))
            )
        );

        return await _reviews.Find(filter).ToListAsync();
    }

    /// <summary>
    /// Aggregate pipeline - середній рейтинг фільму
    /// </summary>
    public async Task<double> GetAverageRatingAsync(long movieId)
    {
        var pipeline = _reviews.Aggregate()
            .Match(r => r.MovieId == movieId && !r.IsDeleted)
            .Group(r => r.MovieId, g => new { AverageRating = g.Average(r => r.Rating) });

        var result = await pipeline.FirstOrDefaultAsync();
        return result?.AverageRating ?? 0;
    }

    /// <summary>
    /// Aggregate - статистика відгуків по фільму
    /// </summary>
    public async Task<ReviewStatistics> GetMovieStatisticsAsync(long movieId)
    {
        var pipeline = _reviews.Aggregate()
            .Match(r => r.MovieId == movieId && !r.IsDeleted)
            .Group(r => r.MovieId, g => new ReviewStatistics
            {
                TotalReviews = g.Count(),
                AverageRating = g.Average(r => r.Rating),
                TotalLikes = g.Sum(r => r.Likes),
                TotalComments = g.Sum(r => r.Comments.Count)
            });

        return await pipeline.FirstOrDefaultAsync() ?? new ReviewStatistics();
    }
}

/// <summary>
/// DTO для статистики
/// </summary>
public class ReviewStatistics
{
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public int TotalLikes { get; set; }
    public int TotalComments { get; set; }
}