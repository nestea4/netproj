using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Entities.Disscution;
using ReviewService.Domain.Entities.Review;
using ReviewService.Domain.ValueObjects;

namespace ReviewService.Infrastructure.Data;

/// <summary>
///MongoDB Context з налаштуванням BSON серіалізації
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);

        //реєстрація BSON serializers
        RegisterBsonSerializers();
        
        //створення індексів
        CreateIndexes();
    }

    public IMongoDatabase Database => _database;

    /// <summary>
    ///реєстрація BSON serializers для Value Objects
    /// </summary>
    private void RegisterBsonSerializers()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Review)))
        {
            BsonSerializer.RegisterSerializer(new RatingBsonSerializer());
            BsonSerializer.RegisterSerializer(new ReviewContentBsonSerializer());
            BsonSerializer.RegisterSerializer(new MovieReferenceBsonSerializer());
            BsonSerializer.RegisterSerializer(new UserReferenceBsonSerializer());
        }
    }

    /// <summary>
    ///створення індексів для оптимізації
    /// </summary>
    private void CreateIndexes()
    {
        var reviews = _database.GetCollection<Review>("Reviews");
        var discussions = _database.GetCollection<Discussion>("Discussions");

        // Індекси для Reviews
        var reviewMovieIdIndex = Builders<Review>.IndexKeys
            .Ascending("movieReference.movieId")
            .Descending(r => r.CreatedAt);
        reviews.Indexes.CreateOne(new CreateIndexModel<Review>(reviewMovieIdIndex));

        var reviewUserIdIndex = Builders<Review>.IndexKeys
            .Ascending("author.userId");
        reviews.Indexes.CreateOne(new CreateIndexModel<Review>(reviewUserIdIndex));

        var reviewRatingIndex = Builders<Review>.IndexKeys
            .Descending(r => r.Rating);
        reviews.Indexes.CreateOne(new CreateIndexModel<Review>(reviewRatingIndex));

        // Text index для пошуку
        var reviewTextIndex = Builders<Review>.IndexKeys
            .Text("reviewContent.title")
            .Text("reviewContent.content");
        reviews.Indexes.CreateOne(new CreateIndexModel<Review>(reviewTextIndex));

        // Індекси для Discussions
        var discussionMovieIdIndex = Builders<Discussion>.IndexKeys
            .Ascending("movieReference.movieId")
            .Descending(d => d.UpdatedAt);
        discussions.Indexes.CreateOne(new CreateIndexModel<Discussion>(discussionMovieIdIndex));

        var discussionPopularityIndex = Builders<Discussion>.IndexKeys
            .Descending(d => d.ViewCount)
            .Descending(d => d.ParticipantCount);
        discussions.Indexes.CreateOne(new CreateIndexModel<Discussion>(discussionPopularityIndex));
    }
}