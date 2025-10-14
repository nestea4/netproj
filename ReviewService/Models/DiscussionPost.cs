using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReviewService.Models;

/// <summary>
/// Пост в обговоренні
/// </summary>
public class DiscussionPost
{
    [BsonElement("postId")]
    public string PostId { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    public long UserId { get; set; }

    [BsonElement("userName")]
    public string UserName { get; set; } = string.Empty;

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("likes")]
    public int Likes { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; }
}