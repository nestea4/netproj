using MongoDB.Driver;
using ReviewService.Models;

namespace ReviewService.Data.Repositories;

/// <summary>
/// Репозиторій для роботи з обговореннями
/// </summary>
public class DiscussionRepository
{
    private readonly IMongoCollection<Discussion> _discussions;

    public DiscussionRepository(MongoDbContext context)
    {
        _discussions = context.Discussions;
    }

    /// <summary>
    /// Створення обговорення
    /// </summary>
    public async Task<Discussion> CreateAsync(Discussion discussion)
    {
        await _discussions.InsertOneAsync(discussion);
        return discussion;
    }

    /// <summary>
    /// Отримання всіх обговорень фільму
    /// </summary>
    public async Task<List<Discussion>> GetByMovieIdAsync(long movieId)
    {
        return await _discussions
            .Find(d => d.MovieId == movieId && !d.IsDeleted)
            .SortByDescending(d => d.IsPinned)
            .ThenByDescending(d => d.UpdatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Додавання поста в обговорення
    /// </summary>
    public async Task<bool> AddPostAsync(string discussionId, DiscussionPost post)
    {
        var update = Builders<Discussion>.Update
            .Push(d => d.Posts, post)
            .Set(d => d.UpdatedAt, DateTime.UtcNow)
            .Inc(d => d.ParticipantCount, 1);

        var result = await _discussions.UpdateOneAsync(d => d.Id == discussionId, update);
        return result.ModifiedCount > 0;
    }

    /// <summary>
    /// Збільшення лічильника переглядів
    /// </summary>
    public async Task<bool> IncrementViewCountAsync(string discussionId)
    {
        var update = Builders<Discussion>.Update.Inc(d => d.ViewCount, 1);
        var result = await _discussions.UpdateOneAsync(d => d.Id == discussionId, update);
        return result.ModifiedCount > 0;
    }

    /// <summary>
    /// Закріплення обговорення
    /// </summary>
    public async Task<bool> PinDiscussionAsync(string discussionId, bool isPinned)
    {
        var update = Builders<Discussion>.Update.Set(d => d.IsPinned, isPinned);
        var result = await _discussions.UpdateOneAsync(d => d.Id == discussionId, update);
        return result.ModifiedCount > 0;
    }

    /// <summary>
    /// Блокування обговорення
    /// </summary>
    public async Task<bool> LockDiscussionAsync(string discussionId, bool isLocked)
    {
        var update = Builders<Discussion>.Update.Set(d => d.IsLocked, isLocked);
        var result = await _discussions.UpdateOneAsync(d => d.Id == discussionId, update);
        return result.ModifiedCount > 0;
    }

    /// <summary>
    /// Видалення обговорення
    /// </summary>
    public async Task<bool> DeleteAsync(string id)
    {
        var update = Builders<Discussion>.Update.Set(d => d.IsDeleted, true);
        var result = await _discussions.UpdateOneAsync(d => d.Id == id, update);
        return result.ModifiedCount > 0;
    }

    /// <summary>
    /// Популярні обговорення
    /// </summary>
    public async Task<List<Discussion>> GetPopularDiscussionsAsync(int limit = 10)
    {
        return await _discussions
            .Find(d => !d.IsDeleted)
            .SortByDescending(d => d.ViewCount)
            .ThenByDescending(d => d.ParticipantCount)
            .Limit(limit)
            .ToListAsync();
    }
    
    public async Task<Discussion?> GetByIdAsync(string id)
    {
        return await _discussions.Find(d => d.Id == id && !d.IsDeleted).FirstOrDefaultAsync();
    }
}
  

