using MongoDB.Bson;
using MongoDB.Driver;

namespace ReviewService.Infrastructure.Data.Seeding;

/// <summary>
/// Міграція даних MongoDB для переходу на Value Objects
/// </summary>
public static class MongoDbMigration
{
    /// <summary>
    /// Виконує міграцію всіх колекцій
    /// </summary>
    public static async Task MigrateToValueObjectsAsync(IMongoDatabase database, ILogger logger)
    {
        logger.LogInformation("Starting MongoDB migration to Value Objects structure...");

        try
        {
            await MigrateReviewsAsync(database, logger);
            await MigrateDiscussionsAsync(database, logger);
            
            logger.LogInformation("MongoDB migration completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MongoDB migration failed!");
            throw;
        }
    }

    /// <summary>
    /// Міграція колекції Reviews
    /// </summary>
    private static async Task MigrateReviewsAsync(IMongoDatabase database, ILogger logger)
    {
        var collection = database.GetCollection<BsonDocument>("Reviews");
        
        // Перевіряємо чи потрібна міграція
        var sampleDoc = await collection.Find(new BsonDocument()).FirstOrDefaultAsync();
        if (sampleDoc == null)
        {
            logger.LogInformation("Reviews collection is empty, skipping migration");
            return;
        }

        // Якщо вже є movieReference, то міграція не потрібна
        if (sampleDoc.Contains("movieReference"))
        {
            logger.LogInformation("Reviews already migrated, skipping");
            return;
        }

        logger.LogInformation("Migrating Reviews collection...");

        // Отримуємо всі документи
        var reviews = await collection.Find(new BsonDocument()).ToListAsync();
        int migratedCount = 0;

        foreach (var review in reviews)
        {
            var update = new BsonDocument();
            var unset = new BsonDocument();

            // 1. Міграція movieId + movieTitle -> movieReference
            if (review.Contains("movieId") && review.Contains("movieTitle"))
            {
                update["movieReference"] = new BsonDocument
                {
                    { "movieId", review["movieId"] },
                    { "movieTitle", review["movieTitle"] }
                };
                unset["movieId"] = "";
                unset["movieTitle"] = "";
            }

            // 2. Міграція userId + userName -> author (UserReference)
            if (review.Contains("userId") && review.Contains("userName"))
            {
                update["author"] = new BsonDocument
                {
                    { "userId", review["userId"] },
                    { "userName", review["userName"] }
                };
                unset["userId"] = "";
                unset["userName"] = "";
            }

            // 3. Міграція title + content + isSpoiler -> reviewContent
            if (review.Contains("title") && review.Contains("content"))
            {
                update["reviewContent"] = new BsonDocument
                {
                    { "title", review["title"] },
                    { "content", review["content"] },
                    { "isSpoiler", review.Contains("isSpoiler") ? review["isSpoiler"] : false }
                };
                unset["title"] = "";
                unset["content"] = "";
                if (review.Contains("isSpoiler"))
                {
                    unset["isSpoiler"] = "";
                }
            }

            // 4. Міграція comments (якщо є)
            if (review.Contains("comments") && review["comments"].IsBsonArray)
            {
                var comments = review["comments"].AsBsonArray;
                var migratedComments = new BsonArray();

                foreach (var comment in comments)
                {
                    if (comment.IsBsonDocument)
                    {
                        var commentDoc = comment.AsBsonDocument;
                        var migratedComment = new BsonDocument();

                        // Копіюємо всі поля
                        foreach (var element in commentDoc.Elements)
                        {
                            migratedComment[element.Name] = element.Value;
                        }

                        // Міграція userId + userName -> author
                        if (commentDoc.Contains("userId") && commentDoc.Contains("userName"))
                        {
                            migratedComment["author"] = new BsonDocument
                            {
                                { "userId", commentDoc["userId"] },
                                { "userName", commentDoc["userName"] }
                            };
                            migratedComment.Remove("userId");
                            migratedComment.Remove("userName");
                        }

                        // Міграція replies (якщо є)
                        if (commentDoc.Contains("replies") && commentDoc["replies"].IsBsonArray)
                        {
                            var replies = commentDoc["replies"].AsBsonArray;
                            var migratedReplies = new BsonArray();

                            foreach (var reply in replies)
                            {
                                if (reply.IsBsonDocument)
                                {
                                    var replyDoc = reply.AsBsonDocument;
                                    var migratedReply = new BsonDocument();

                                    foreach (var element in replyDoc.Elements)
                                    {
                                        migratedReply[element.Name] = element.Value;
                                    }

                                    if (replyDoc.Contains("userId") && replyDoc.Contains("userName"))
                                    {
                                        migratedReply["author"] = new BsonDocument
                                        {
                                            { "userId", replyDoc["userId"] },
                                            { "userName", replyDoc["userName"] }
                                        };
                                        migratedReply.Remove("userId");
                                        migratedReply.Remove("userName");
                                    }

                                    migratedReplies.Add(migratedReply);
                                }
                            }

                            migratedComment["replies"] = migratedReplies;
                        }

                        migratedComments.Add(migratedComment);
                    }
                }

                update["comments"] = migratedComments;
            }

            // Виконуємо оновлення
            if (update.ElementCount > 0)
            {
                var updateDefinition = Builders<BsonDocument>.Update
                    .Set("$set", update);

                if (unset.ElementCount > 0)
                {
                    updateDefinition = updateDefinition.Set("$unset", unset);
                }

                await collection.UpdateOneAsync(
                    Builders<BsonDocument>.Filter.Eq("_id", review["_id"]),
                    new BsonDocument
                    {
                        { "$set", update },
                        { "$unset", unset }
                    }
                );

                migratedCount++;
            }
        }

        logger.LogInformation("Migrated {Count} reviews", migratedCount);
    }

    /// <summary>
    /// Міграція колекції Discussions
    /// </summary>
    private static async Task MigrateDiscussionsAsync(IMongoDatabase database, ILogger logger)
    {
        var collection = database.GetCollection<BsonDocument>("Discussions");
        
        // Перевіряємо чи потрібна міграція
        var sampleDoc = await collection.Find(new BsonDocument()).FirstOrDefaultAsync();
        if (sampleDoc == null)
        {
            logger.LogInformation("Discussions collection is empty, skipping migration");
            return;
        }

        // Якщо вже є movieReference, то міграція не потрібна
        if (sampleDoc.Contains("movieReference"))
        {
            logger.LogInformation("Discussions already migrated, skipping");
            return;
        }

        logger.LogInformation("Migrating Discussions collection...");

        var discussions = await collection.Find(new BsonDocument()).ToListAsync();
        int migratedCount = 0;

        foreach (var discussion in discussions)
        {
            var update = new BsonDocument();
            var unset = new BsonDocument();

            // 1. Міграція movieId + movieTitle -> movieReference
            if (discussion.Contains("movieId") && discussion.Contains("movieTitle"))
            {
                update["movieReference"] = new BsonDocument
                {
                    { "movieId", discussion["movieId"] },
                    { "movieTitle", discussion["movieTitle"] }
                };
                unset["movieId"] = "";
                unset["movieTitle"] = "";
            }

            // 2. Міграція createdBy + createdByName -> creator
            if (discussion.Contains("createdBy") && discussion.Contains("createdByName"))
            {
                update["creator"] = new BsonDocument
                {
                    { "userId", discussion["createdBy"] },
                    { "userName", discussion["createdByName"] }
                };
                unset["createdBy"] = "";
                unset["createdByName"] = "";
            }

            // 3. Міграція posts (якщо є)
            if (discussion.Contains("posts") && discussion["posts"].IsBsonArray)
            {
                var posts = discussion["posts"].AsBsonArray;
                var migratedPosts = new BsonArray();

                foreach (var post in posts)
                {
                    if (post.IsBsonDocument)
                    {
                        var postDoc = post.AsBsonDocument;
                        var migratedPost = new BsonDocument();

                        // Копіюємо всі поля
                        foreach (var element in postDoc.Elements)
                        {
                            migratedPost[element.Name] = element.Value;
                        }

                        // Міграція userId + userName -> author
                        if (postDoc.Contains("userId") && postDoc.Contains("userName"))
                        {
                            migratedPost["author"] = new BsonDocument
                            {
                                { "userId", postDoc["userId"] },
                                { "userName", postDoc["userName"] }
                            };
                            migratedPost.Remove("userId");
                            migratedPost.Remove("userName");
                        }

                        migratedPosts.Add(migratedPost);
                    }
                }

                update["posts"] = migratedPosts;
            }

            // Виконуємо оновлення
            if (update.ElementCount > 0)
            {
                await collection.UpdateOneAsync(
                    Builders<BsonDocument>.Filter.Eq("_id", discussion["_id"]),
                    new BsonDocument
                    {
                        { "$set", update },
                        { "$unset", unset }
                    }
                );

                migratedCount++;
            }
        }

        logger.LogInformation("Migrated {Count} discussions", migratedCount);
    }

    /// <summary>
    /// Видалення всіх даних (для повного ресіду)
    /// </summary>
    public static async Task ClearAllDataAsync(IMongoDatabase database, ILogger logger)
    {
        logger.LogWarning("Clearing all data from MongoDB...");

        try
        {
            // Видаляємо всі документи
            await database.GetCollection<BsonDocument>("Reviews").DeleteManyAsync(new BsonDocument());
            await database.GetCollection<BsonDocument>("Discussions").DeleteManyAsync(new BsonDocument());

            logger.LogInformation("All data cleared from MongoDB");
            
            // Також видаляємо індекси (крім _id)
            try
            {
                await database.GetCollection<BsonDocument>("Reviews").Indexes.DropAllAsync();
                await database.GetCollection<BsonDocument>("Discussions").Indexes.DropAllAsync();
                logger.LogInformation("All indexes dropped");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not drop indexes (this is OK)");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error clearing MongoDB data");
            throw;
        }
    }
}