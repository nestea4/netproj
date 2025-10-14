using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReviewService.Models;

/// <summary>
/// Обговорення фільму (окрема колекція)
/// Демонструє: Reference до Review, гнучку схему
/// </summary>
public class Discussion
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("movieId")]
    public long MovieId { get; set; }

    [BsonElement("movieTitle")]
    public string MovieTitle { get; set; } = string.Empty;

    [BsonElement("topic")]
    public string Topic { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("createdBy")]
    public long CreatedBy { get; set; }

    [BsonElement("createdByName")]
    public string CreatedByName { get; set; } = string.Empty;

    // Embedded posts
    [BsonElement("posts")]
    public List<DiscussionPost> Posts { get; set; } = new();

    [BsonElement("participantCount")]
    public int ParticipantCount { get; set; }

    [BsonElement("viewCount")]
    public int ViewCount { get; set; }

    [BsonElement("isPinned")]
    public bool IsPinned { get; set; }

    [BsonElement("isLocked")]
    public bool IsLocked { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; }
}