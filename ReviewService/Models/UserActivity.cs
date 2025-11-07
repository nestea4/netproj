using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReviewService.Models;

/// <summary>
/// Активність користувача (окрема колекція)
/// Демонстру: гнучку схему, різні типи активності
/// </summary>
public class UserActivity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    public long UserId { get; set; }

    [BsonElement("activityType")]
    public string ActivityType { get; set; } = string.Empty; // "Like","Dislike","View","Comment"

    [BsonElement("targetType")]
    public string TargetType { get; set; } = string.Empty; // "Review","Discussion","Comment"

    [BsonElement("targetId")]
    public string TargetId { get; set; } = string.Empty;

    //гнучка схема різні поля для різних типів активності
    [BsonElement("metadata")]
    [BsonExtraElements]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}