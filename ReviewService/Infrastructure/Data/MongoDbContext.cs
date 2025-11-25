using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using ReviewService.Domain.Common;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Entities.Disscution;
using ReviewService.Domain.Entities.Review;
using ReviewService.Domain.ValueObjects;

namespace ReviewService.Infrastructure.Data;

/*/// <summary>
/// MongoDB Context з налаштуванням BSON серіалізації
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);

        // Реєстрація BSON serializers
        RegisterBsonSerializers();
        
        // Створення індексів
        CreateIndexes();
    }

    public IMongoDatabase Database => _database;

    /// <summary>
    /// Реєстрація BSON serializers для Value Objects та Entities
    /// </summary>
    private void RegisterBsonSerializers()
    {
        // ==========================================
        // BASE ENTITY - ОБОВ'ЯЗКОВО СПОЧАТКУ!
        // ==========================================
        
        if (!BsonClassMap.IsClassMapRegistered(typeof(BaseEntity)))
        {
            BsonClassMap.RegisterClassMap<BaseEntity>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.SetIsRootClass(true); // Це кореневий клас
                
                // Мапінг Id як ObjectId для базового класу
                cm.MapIdMember(x => x.Id)
                    .SetSerializer(new StringSerializer(BsonType.ObjectId))
                    .SetIdGenerator(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator.Instance);
                    
                cm.MapMember(x => x.CreatedAt)
                    .SetElementName("createdAt");
                    
                cm.MapMember(x => x.UpdatedAt)
                    .SetElementName("updatedAt")
                    .SetIgnoreIfNull(true);
            });
        }

        // ==========================================
        // VALUE OBJECTS - BsonClassMap реєстрація
        // ==========================================
        
        if (!BsonClassMap.IsClassMapRegistered(typeof(MovieReference)))
        {
            BsonClassMap.RegisterClassMap<MovieReference>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                
                cm.MapMember(x => x.MovieId)
                    .SetElementName("movieId")
                    .SetIsRequired(true);
                    
                cm.MapMember(x => x.MovieTitle)
                    .SetElementName("movieTitle")
                    .SetIsRequired(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(UserReference)))
        {
            BsonClassMap.RegisterClassMap<UserReference>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                
                cm.MapMember(x => x.UserId)
                    .SetElementName("userId")
                    .SetIsRequired(true);
                    
                cm.MapMember(x => x.UserName)
                    .SetElementName("userName")
                    .SetIsRequired(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(ReviewContent)))
        {
            BsonClassMap.RegisterClassMap<ReviewContent>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                
                cm.MapMember(x => x.Title)
                    .SetElementName("title")
                    .SetIsRequired(true);
                    
                cm.MapMember(x => x.Content)
                    .SetElementName("content")
                    .SetIsRequired(true);
                    
                cm.MapMember(x => x.IsSpoiler)
                    .SetElementName("isSpoiler");
            });
        }

        // Rating - залишаємо кастомний serializer
        if (!BsonClassMap.IsClassMapRegistered(typeof(Rating)))
        {
            BsonSerializer.RegisterSerializer(new RatingBsonSerializer());
        }

        // ==========================================
        // DOMAIN ENTITIES - без MapIdMember!
        // ==========================================
        
        if (!BsonClassMap.IsClassMapRegistered(typeof(Review)))
        {
            BsonClassMap.RegisterClassMap<Review>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.SetIsRootClass(false); // Це НЕ кореневий клас
                // НЕ викликаємо MapIdMember - воно вже є в BaseEntity!
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Discussion)))
        {
            BsonClassMap.RegisterClassMap<Discussion>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.SetIsRootClass(false); // Це НЕ кореневий клас
                // НЕ викликаємо MapIdMember - воно вже є в BaseEntity!
            });
        }

        // ==========================================
        // EMBEDDED ENTITIES
        // ==========================================
        
        if (!BsonClassMap.IsClassMapRegistered(typeof(Comment)))
        {
            BsonClassMap.RegisterClassMap<Comment>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(CommentReply)))
        {
            BsonClassMap.RegisterClassMap<CommentReply>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(DiscussionPost)))
        {
            BsonClassMap.RegisterClassMap<DiscussionPost>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(ReviewStatistics)))
        {
            BsonClassMap.RegisterClassMap<ReviewStatistics>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }

    /// <summary>
    /// Створення індексів для оптимізації
    /// </summary>
    private void CreateIndexes()
    {
        var reviews = _database.GetCollection<Review>("Reviews");
        var discussions = _database.GetCollection<Discussion>("Discussions");

        // ==============================================
        // REVIEWS INDEXES
        // ==============================================
        
        // Отримуємо існуючі індекси
        var existingReviewIndexes = reviews.Indexes.List().ToList()
            .Select(idx => idx["name"].AsString)
            .ToHashSet();

        // Compound index: movieId + createdAt
        if (!existingReviewIndexes.Contains("idx_movie_created"))
        {
            try
            {
                var reviewMovieIdIndex = Builders<Review>.IndexKeys
                    .Ascending("movieReference.movieId")
                    .Descending(r => r.CreatedAt);
                reviews.Indexes.CreateOne(
                    new CreateIndexModel<Review>(
                        reviewMovieIdIndex,
                        new CreateIndexOptions { Name = "idx_movie_created" }
                    )
                );
            }
            catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
            {
                // Індекс вже існує з іншим ім'ям - ігноруємо
            }
        }

        // Index: userId
        if (!existingReviewIndexes.Contains("idx_author_user"))
        {
            try
            {
                var reviewUserIdIndex = Builders<Review>.IndexKeys
                    .Ascending("author.userId");
                reviews.Indexes.CreateOne(
                    new CreateIndexModel<Review>(
                        reviewUserIdIndex,
                        new CreateIndexOptions { Name = "idx_author_user" }
                    )
                );
            }
            catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
            {
                // Індекс вже існує
            }
        }

        // Index: rating
        if (!existingReviewIndexes.Contains("idx_rating"))
        {
            try
            {
                var reviewRatingIndex = Builders<Review>.IndexKeys
                    .Descending(r => r.Rating);
                reviews.Indexes.CreateOne(
                    new CreateIndexModel<Review>(
                        reviewRatingIndex,
                        new CreateIndexOptions { Name = "idx_rating" }
                    )
                );
            }
            catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
            {
                // Індекс вже існує
            }
        }

        // Text index для full-text search
        if (!existingReviewIndexes.Contains("idx_text_search"))
        {
            try
            {
                var reviewTextIndex = Builders<Review>.IndexKeys
                    .Text("reviewContent.title")
                    .Text("reviewContent.content");
                reviews.Indexes.CreateOne(
                    new CreateIndexModel<Review>(
                        reviewTextIndex,
                        new CreateIndexOptions { Name = "idx_text_search" }
                    )
                );
            }
            catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
            {
                // Індекс вже існує
            }
        }

        // ==============================================
        // DISCUSSIONS INDEXES
        // ==============================================
        
        var existingDiscussionIndexes = discussions.Indexes.List().ToList()
            .Select(idx => idx["name"].AsString)
            .ToHashSet();

        // Compound index: movieId + updatedAt
        if (!existingDiscussionIndexes.Contains("idx_movie_updated"))
        {
            try
            {
                var discussionMovieIdIndex = Builders<Discussion>.IndexKeys
                    .Ascending("movieReference.movieId")
                    .Descending(d => d.UpdatedAt);
                discussions.Indexes.CreateOne(
                    new CreateIndexModel<Discussion>(
                        discussionMovieIdIndex,
                        new CreateIndexOptions { Name = "idx_movie_updated" }
                    )
                );
            }
            catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
            {
                // Індекс вже існує
            }
        }

        // Compound index для популярності
        if (!existingDiscussionIndexes.Contains("idx_popularity"))
        {
            try
            {
                var discussionPopularityIndex = Builders<Discussion>.IndexKeys
                    .Descending(d => d.ViewCount)
                    .Descending(d => d.ParticipantCount);
                discussions.Indexes.CreateOne(
                    new CreateIndexModel<Discussion>(
                        discussionPopularityIndex,
                        new CreateIndexOptions { Name = "idx_popularity" }
                    )
                );
            }
            catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
            {
                // Індекс вже існує
            }
        }

        // Index: isPinned
        if (!existingDiscussionIndexes.Contains("idx_pinned"))
        {
            try
            {
                var discussionPinnedIndex = Builders<Discussion>.IndexKeys
                    .Descending(d => d.IsPinned)
                    .Descending(d => d.UpdatedAt);
                discussions.Indexes.CreateOne(
                    new CreateIndexModel<Discussion>(
                        discussionPinnedIndex,
                        new CreateIndexOptions { Name = "idx_pinned" }
                    )
                );
            }
            catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
            {
                // Індекс вже існує
            }
        }
    }
}*/

/// <summary>
/// MongoDB Context з налаштуванням BSON серіалізації
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);

        // Реєстрація BSON serializers
        RegisterBsonSerializers();
        
        // Створення індексів
        CreateIndexes();
    }

    public IMongoDatabase Database => _database;

    /// <summary>
    /// Реєстрація BSON serializers для Value Objects та Entities
    /// </summary>
    private void RegisterBsonSerializers()
    {
        // ==========================================
        // BASE ENTITY - ОБОВ'ЯЗКОВО СПОЧАТКУ!
        // ==========================================
        
        if (!BsonClassMap.IsClassMapRegistered(typeof(BaseEntity)))
        {
            BsonClassMap.RegisterClassMap<BaseEntity>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.SetIsRootClass(true);
                
                // ВАЖЛИВО: Вимикаємо discriminator (_t поле)
                cm.SetDiscriminator("BaseEntity");
                cm.SetDiscriminatorIsRequired(false);
                
                // Мапінг Id як ObjectId
                cm.MapIdMember(x => x.Id)
                    .SetSerializer(new StringSerializer(BsonType.ObjectId))
                    .SetIdGenerator(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator.Instance);
                    
                cm.MapMember(x => x.CreatedAt)
                    .SetElementName("createdAt");
                    
                cm.MapMember(x => x.UpdatedAt)
                    .SetElementName("updatedAt")
                    .SetIgnoreIfNull(true);
            });
        }

        // ==========================================
        // VALUE OBJECTS - BsonClassMap реєстрація
        // ==========================================
        
        if (!BsonClassMap.IsClassMapRegistered(typeof(MovieReference)))
        {
            BsonClassMap.RegisterClassMap<MovieReference>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                
                // Для нової структури (вкладений об'єкт)
                cm.MapMember(x => x.MovieId)
                    .SetElementName("movieId")
                    .SetIsRequired(true);
                    
                cm.MapMember(x => x.MovieTitle)
                    .SetElementName("movieTitle")
                    .SetIsRequired(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(UserReference)))
        {
            BsonClassMap.RegisterClassMap<UserReference>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                
                // Для нової структури (вкладений об'єкт)
                cm.MapMember(x => x.UserId)
                    .SetElementName("userId")
                    .SetIsRequired(true);
                    
                cm.MapMember(x => x.UserName)
                    .SetElementName("userName")
                    .SetIsRequired(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(ReviewContent)))
        {
            BsonClassMap.RegisterClassMap<ReviewContent>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                
                cm.MapMember(x => x.Title)
                    .SetElementName("title")
                    .SetIsRequired(true);
                    
                cm.MapMember(x => x.Content)
                    .SetElementName("content")
                    .SetIsRequired(true);
                    
                cm.MapMember(x => x.IsSpoiler)
                    .SetElementName("isSpoiler");
            });
        }

        // Rating - залишаємо кастомний serializer
        if (!BsonClassMap.IsClassMapRegistered(typeof(Rating)))
        {
            BsonSerializer.RegisterSerializer(new RatingBsonSerializer());
        }

        // ==========================================
        // DOMAIN ENTITIES - без MapIdMember!
        // ==========================================
        
        if (!BsonClassMap.IsClassMapRegistered(typeof(Review)))
        {
            BsonClassMap.RegisterClassMap<Review>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.SetIsRootClass(false); // Це НЕ кореневий клас
                // НЕ викликаємо MapIdMember - воно вже є в BaseEntity!
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Discussion)))
        {
            BsonClassMap.RegisterClassMap<Discussion>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.SetIsRootClass(false);
                
                // Мапінг для movieReference (вкладений об'єкт)
                cm.MapMember(x => x.MovieReference)
                    .SetElementName("movieReference");
                    
                // Мапінг для creator (вкладений об'єкт)
                cm.MapMember(x => x.Creator)
                    .SetElementName("creator");
            });
        }

        // ==========================================
        // EMBEDDED ENTITIES
        // ==========================================
        
        if (!BsonClassMap.IsClassMapRegistered(typeof(Comment)))
        {
            BsonClassMap.RegisterClassMap<Comment>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                
                // Мапінг для author (вкладений об'єкт)
                cm.MapMember(x => x.Author)
                    .SetElementName("author");
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(CommentReply)))
        {
            BsonClassMap.RegisterClassMap<CommentReply>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                
                // Мапінг для author (вкладений об'єкт)
                cm.MapMember(x => x.Author)
                    .SetElementName("author");
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(DiscussionPost)))
        {
            BsonClassMap.RegisterClassMap<DiscussionPost>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                
                // Мапінг для author (вкладений об'єкт)
                cm.MapMember(x => x.Author)
                    .SetElementName("author");
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(ReviewStatistics)))
        {
            BsonClassMap.RegisterClassMap<ReviewStatistics>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }

    /// <summary>
    /// Створення індексів для оптимізації
    /// </summary>
    private void CreateIndexes()
    {
        var reviews = _database.GetCollection<Review>("Reviews");
        var discussions = _database.GetCollection<Discussion>("Discussions");

        // ==============================================
        // REVIEWS INDEXES
        // ==============================================
        
        // Отримуємо існуючі індекси
        var existingReviewIndexes = reviews.Indexes.List().ToList()
            .Select(idx => idx["name"].AsString)
            .ToHashSet();

        // Compound index: movieId + createdAt
        if (!existingReviewIndexes.Contains("idx_movie_created"))
        {
            try
            {
                var reviewMovieIdIndex = Builders<Review>.IndexKeys
                    .Ascending("movieReference.movieId")
                    .Descending(r => r.CreatedAt);
                reviews.Indexes.CreateOne(
                    new CreateIndexModel<Review>(
                        reviewMovieIdIndex,
                        new CreateIndexOptions { Name = "idx_movie_created" }
                    )
                );
            }
            catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
            {
                // Індекс вже існує з іншим ім'ям - ігноруємо
            }
        }

        // Index: userId
        if (!existingReviewIndexes.Contains("idx_author_user"))
        {
            try
            {
                var reviewUserIdIndex = Builders<Review>.IndexKeys
                    .Ascending("author.userId");
                reviews.Indexes.CreateOne(
                    new CreateIndexModel<Review>(
                        reviewUserIdIndex,
                        new CreateIndexOptions { Name = "idx_author_user" }
                    )
                );
            }
            catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
            {
                // Індекс вже існує
            }
        }

        // Index: rating
        if (!existingReviewIndexes.Contains("idx_rating"))
        {
            try
            {
                var reviewRatingIndex = Builders<Review>.IndexKeys
                    .Descending(r => r.Rating);
                reviews.Indexes.CreateOne(
                    new CreateIndexModel<Review>(
                        reviewRatingIndex,
                        new CreateIndexOptions { Name = "idx_rating" }
                    )
                );
            }
            catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
            {
                // Індекс вже існує
            }
        }

        // Text index для full-text search
        if (!existingReviewIndexes.Contains("idx_text_search"))
        {
            try
            {
                var reviewTextIndex = Builders<Review>.IndexKeys
                    .Text("reviewContent.title")
                    .Text("reviewContent.content");
                reviews.Indexes.CreateOne(
                    new CreateIndexModel<Review>(
                        reviewTextIndex,
                        new CreateIndexOptions { Name = "idx_text_search" }
                    )
                );
            }
            catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
            {
                // Індекс вже існує
            }
        }

        // ==============================================
        // DISCUSSIONS INDEXES
        // ==============================================
        
        var existingDiscussionIndexes = discussions.Indexes.List().ToList()
            .Select(idx => idx["name"].AsString)
            .ToHashSet();

        // Compound index: movieId + updatedAt
        if (!existingDiscussionIndexes.Contains("idx_movie_updated"))
        {
            try
            {
                var discussionMovieIdIndex = Builders<Discussion>.IndexKeys
                    .Ascending("movieReference.movieId")
                    .Descending(d => d.UpdatedAt);
                discussions.Indexes.CreateOne(
                    new CreateIndexModel<Discussion>(
                        discussionMovieIdIndex,
                        new CreateIndexOptions { Name = "idx_movie_updated" }
                    )
                );
            }
            catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
            {
                // Індекс вже існує
            }
        }

        // Compound index для популярності
        if (!existingDiscussionIndexes.Contains("idx_popularity"))
        {
            try
            {
                var discussionPopularityIndex = Builders<Discussion>.IndexKeys
                    .Descending(d => d.ViewCount)
                    .Descending(d => d.ParticipantCount);
                discussions.Indexes.CreateOne(
                    new CreateIndexModel<Discussion>(
                        discussionPopularityIndex,
                        new CreateIndexOptions { Name = "idx_popularity" }
                    )
                );
            }
            catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
            {
                // Індекс вже існує
            }
        }

        // Index: isPinned
        if (!existingDiscussionIndexes.Contains("idx_pinned"))
        {
            try
            {
                var discussionPinnedIndex = Builders<Discussion>.IndexKeys
                    .Descending(d => d.IsPinned)
                    .Descending(d => d.UpdatedAt);
                discussions.Indexes.CreateOne(
                    new CreateIndexModel<Discussion>(
                        discussionPinnedIndex,
                        new CreateIndexOptions { Name = "idx_pinned" }
                    )
                );
            }
            catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
            {
                // Індекс вже існує
            }
        }
    }
}