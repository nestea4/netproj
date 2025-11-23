using MongoDB.Driver;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Entities.Disscution;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Infrastructure.Data.Repositories;

/// <summary>
///MongoDB репозиторій для обговорень
/// </summary>
public class MongoDiscussionRepository : IDiscussionRepository
{
    private readonly IMongoCollection<Discussion> _discussions;

    public MongoDiscussionRepository(IMongoDatabase database)
    {
        _discussions = database.GetCollection<Discussion>("Discussions");
    }

    public async Task<Discussion?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _discussions
            .Find(d => d.Id == id && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Discussion> CreateAsync(Discussion entity, CancellationToken cancellationToken = default)
    {
        await _discussions.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    public async Task<bool> UpdateAsync(Discussion entity, CancellationToken cancellationToken = default)
    {
        var result = await _discussions.ReplaceOneAsync(
            d => d.Id == entity.Id,
            entity,
            cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var update = Builders<Discussion>.Update.Set(d => d.IsDeleted, true);
        var result = await _discussions.UpdateOneAsync(
            d => d.Id == id,
            update,
            cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }

    public async Task<List<Discussion>> GetByMovieIdAsync(long movieId, CancellationToken cancellationToken = default)
    {
        return await _discussions
            .Find(d => d.MovieReference.MovieId == movieId && !d.IsDeleted)
            .SortByDescending(d => d.IsPinned)
            .ThenByDescending(d => d.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///Aggregation для популярних обговорень
    /// </summary>
    public async Task<List<Discussion>> GetPopularDiscussionsAsync(int limit, CancellationToken cancellationToken = default)
    {
        return await _discussions
            .Find(d => !d.IsDeleted)
            .SortByDescending(d => d.ViewCount)
            .ThenByDescending(d => d.ParticipantCount)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Discussion>> GetPinnedDiscussionsAsync(CancellationToken cancellationToken = default)
    {
        return await _discussions
            .Find(d => d.IsPinned && !d.IsDeleted)
            .SortByDescending(d => d.UpdatedAt)
            .ToListAsync(cancellationToken);
    }
}