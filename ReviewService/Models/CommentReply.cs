using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReviewService.Models;

/// <summary>
/// Відповідь на коментар
/// </summary>
public class CommentReply
{
    [BsonElement("replyId")]
    public string ReplyId { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    public long UserId { get; set; }

    [BsonElement("userName")]
    public string UserName { get; set; } = string.Empty;

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}