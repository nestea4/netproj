using MongoDB.Driver;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Entities.Review;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Infrastructure.Data.Repositories;

/// <summary>
///MongoDB репозиторій для відгуків з aggregation pipeline
/// </summary>
public class MongoReviewRepository : IReviewRepository
{
    private readonly IMongoCollection<Review> _reviews;

    public MongoReviewRepository(IMongoDatabase database)
    {
        _reviews = database.GetCollection<Review>("Reviews");
    }

    public async Task<Review?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _reviews
            .Find(r => r.Id == id && !r.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Review> CreateAsync(Review entity, CancellationToken cancellationToken = default)
    {
        await _reviews.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    public async Task<bool> UpdateAsync(Review entity, CancellationToken cancellationToken = default)
    {
        var result = await _reviews.ReplaceOneAsync(
            r => r.Id == entity.Id,
            entity,
            cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var update = Builders<Review>.Update.Set(r => r.IsDeleted, true);
        var result = await _reviews.UpdateOneAsync(
            r => r.Id == id,
            update,
            cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }

    public async Task<List<Review>> GetByMovieIdAsync(long movieId, CancellationToken cancellationToken = default)
    {
        return await _reviews
            .Find(r => r.MovieReference.MovieId == movieId && !r.IsDeleted)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Review>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _reviews
            .Find(r => r.Author.UserId == userId && !r.IsDeleted)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Review>> GetTopReviewsAsync(long movieId, int limit, CancellationToken cancellationToken = default)
    {
        return await _reviews
            .Find(r => r.MovieReference.MovieId == movieId && !r.IsDeleted)
            .SortByDescending(r => r.Statistics.Likes)
            .ThenByDescending(r => r.Rating)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Aggregation pipeline для середнього рейтингу
    /// </summary>
    public async Task<double> GetAverageRatingAsync(long movieId, CancellationToken cancellationToken = default)
    {
        var pipeline = _reviews.Aggregate()
            .Match(r => r.MovieReference.MovieId == movieId && !r.IsDeleted)
            .Group(
                r => r.MovieReference.MovieId,
                g => new { AverageRating = g.Average(r => (int)r.Rating) });

        var result = await pipeline.FirstOrDefaultAsync(cancellationToken);
        return result?.AverageRating ?? 0;
    }

    /// <summary>
    /// Aggregation pipeline для статистики фільму
    /// </summary>
    public async Task<ReviewStatisticsDto> GetMovieStatisticsAsync(long movieId, CancellationToken cancellationToken = default)
    {
        var pipeline = _reviews.Aggregate()
            .Match(r => r.MovieReference.MovieId == movieId && !r.IsDeleted)
            .Group(
                r => r.MovieReference.MovieId,
                g => new ReviewStatisticsDto
                {
                    TotalReviews = g.Count(),
                    AverageRating = g.Average(r => (int)r.Rating),
                    TotalLikes = g.Sum(r => r.Statistics.Likes),
                    TotalComments = g.Sum(r => r.Statistics.CommentCount)
                });

        return await pipeline.FirstOrDefaultAsync(cancellationToken) ?? new ReviewStatisticsDto();
    }

    /// <summary>
    ///Текстовий пошук
    /// </summary>
    public async Task<List<Review>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Review>.Filter.And(
            Builders<Review>.Filter.Eq(r => r.IsDeleted, false),
            Builders<Review>.Filter.Or(
                Builders<Review>.Filter.Regex(
                    r => r.ReviewContent.Title,
                    new MongoDB.Bson.BsonRegularExpression(query, "i")),
                Builders<Review>.Filter.Regex(
                    r => r.ReviewContent.Content,
                    new MongoDB.Bson.BsonRegularExpression(query, "i"))
            )
        );

        return await _reviews.Find(filter).ToListAsync(cancellationToken);
    }
}