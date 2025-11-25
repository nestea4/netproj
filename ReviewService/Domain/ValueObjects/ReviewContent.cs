using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using ReviewService.Domain.Common;
using ReviewService.Domain.Exceptions;

namespace ReviewService.Domain.ValueObjects;

/// <summary>
/// Value Object для контенту відгуку
/// </summary>
public class ReviewContent : ValueObject
{
    [BsonElement("title")]
    public string Title { get; internal set; }
    
    [BsonElement("content")]
    public string Content { get; internal set; }
    
    [BsonElement("isSpoiler")]
    public bool IsSpoiler { get; internal set; }

    // Parameterless constructor для BSON десеріалізації
    private ReviewContent() 
    { 
        Title = string.Empty;
        Content = string.Empty;
    }

    private ReviewContent(string title, string content, bool isSpoiler)
    {
        Title = title;
        Content = content;
        IsSpoiler = isSpoiler;
    }

    public static ReviewContent Create(string title, string content, bool isSpoiler = false)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Review title cannot be empty");

        if (title.Length > 200)
            throw new DomainException("Review title cannot exceed 200 characters");

        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Review content cannot be empty");

        if (content.Length < 50)
            throw new DomainException("Review content must be at least 50 characters");

        if (content.Length > 5000)
            throw new DomainException("Review content cannot exceed 5000 characters");

        return new ReviewContent(title, content, isSpoiler);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Title;
        yield return Content;
        yield return IsSpoiler;
    }
}