namespace ReviewService.Configuration;

/// <summary>
/// налаштування підключення до MongoDB
/// </summary>
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "CinemaReviewDB";
    public string ReviewsCollectionName { get; set; } = "Reviews";
    public string DiscussionsCollectionName { get; set; } = "Discussions";
    public string UserActivityCollectionName { get; set; } = "UserActivity";
}