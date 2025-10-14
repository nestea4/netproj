using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReviewService.Models;

/// <summary>
/// Відгук про фільм (Root Document)
/// Демонструє: Embedded documents (Comments), References (MovieId, UserId)
/// </summary>
public class Review
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    // Reference до фільму з Catalog Service
    [BsonElement("movieId")]
    public long MovieId { get; set; }
    
    // Денормалізація для швидкого читання
    [BsonElement("movieTitle")]
    public string MovieTitle { get; set; } = string.Empty;

    // Reference до користувача з Booking Service
    [BsonElement("userId")]
    public long UserId { get; set; }
    
    // Денормалізація
    [BsonElement("userName")]
    public string UserName { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("rating")]
    [BsonRepresentation(BsonType.Int32)]
    public int Rating { get; set; } // 1-10

    [BsonElement("reviewType")]
    public string ReviewType { get; set; } = "Standard"; // Standard, Detailed, Quick

    // Embedded document - масив коментарів
    [BsonElement("comments")]
    public List<Comment> Comments { get; set; } = new();

    // Статистика (денормалізація)
    [BsonElement("likes")]
    public int Likes { get; set; }

    [BsonElement("dislikes")]
    public int Dislikes { get; set; }

    [BsonElement("viewCount")]
    public int ViewCount { get; set; }

    // Метадані
    [BsonElement("isVerifiedPurchase")]
    public bool IsVerifiedPurchase { get; set; } // Чи купив квиток

    [BsonElement("isSpoiler")]
    public bool IsSpoiler { get; set; }

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new(); // "Masterpiece", "Boring", "Visually Stunning"

    // Аудит
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; }
}