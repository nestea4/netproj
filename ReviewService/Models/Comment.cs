using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReviewService.Models;

/// <summary>
/// Коментар до відгуку (Embedded document)
/// тут вкладені документи та масиви
/// </summary>
public class Comment
{
    [BsonElement("commentId")]
    public string CommentId { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    public long UserId { get; set; }

    [BsonElement("userName")]
    public string UserName { get; set; } = string.Empty;

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("likes")]
    public int Likes { get; set; }

    //вкладені відповіді на коментар (nested array)
    [BsonElement("replies")]
    public List<CommentReply> Replies { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; }
}