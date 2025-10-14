namespace ReviewService.Configuration;

/// <summary>
/// Налаштування підключення до MongoDB
/// </summary>
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "CinemaReviewDB";
    public string ReviewsCollectionName { get; set; } = "Reviews";
    public string DiscussionsCollectionName { get; set; } = "Discussions";
    public string UserActivityCollectionName { get; set; } = "UserActivity";
}