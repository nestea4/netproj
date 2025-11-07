using MongoDB.Driver;
using ReviewService.Configuration;
using ReviewService.Models;

namespace ReviewService.Data;

/// <summary>
/// MongoDB Context для доступу до колекцій
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
        
        //створення індексів при ініціалізації
        CreateIndexes();
    }

    //колекції
    public IMongoCollection<Review> Reviews => _database.GetCollection<Review>("Reviews");
    public IMongoCollection<Discussion> Discussions => _database.GetCollection<Discussion>("Discussions");
    public IMongoCollection<UserActivity> UserActivities => _database.GetCollection<UserActivity>("UserActivity");

    /// <summary>
    /// Створення індексів для оптимізації запитів
    /// </summary>
    private void CreateIndexes()
    {
        //індекси для Reviews
        var reviewIndexKeys = Builders<Review>.IndexKeys
            .Ascending(r => r.MovieId)
            .Descending(r => r.CreatedAt);
        Reviews.Indexes.CreateOne(new CreateIndexModel<Review>(reviewIndexKeys));

        var reviewRatingIndex = Builders<Review>.IndexKeys.Descending(r => r.Rating);
        Reviews.Indexes.CreateOne(new CreateIndexModel<Review>(reviewRatingIndex));

        var reviewUserIndex = Builders<Review>.IndexKeys.Ascending(r => r.UserId);
        Reviews.Indexes.CreateOne(new CreateIndexModel<Review>(reviewUserIndex));

        //індекси для Discussions
        var discussionMovieIndex = Builders<Discussion>.IndexKeys
            .Ascending(d => d.MovieId)
            .Descending(d => d.CreatedAt);
        Discussions.Indexes.CreateOne(new CreateIndexModel<Discussion>(discussionMovieIndex));

        //індекси для UserActivity
        var activityUserIndex = Builders<UserActivity>.IndexKeys
            .Ascending(a => a.UserId)
            .Descending(a => a.CreatedAt);
        UserActivities.Indexes.CreateOne(new CreateIndexModel<UserActivity>(activityUserIndex));

        var activityTargetIndex = Builders<UserActivity>.IndexKeys
            .Ascending(a => a.TargetType)
            .Ascending(a => a.TargetId);
        UserActivities.Indexes.CreateOne(new CreateIndexModel<UserActivity>(activityTargetIndex));
    }
}