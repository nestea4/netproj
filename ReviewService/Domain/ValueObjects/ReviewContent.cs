using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using ReviewService.Domain.Common;
using ReviewService.Domain.Exceptions;

namespace ReviewService.Domain.ValueObjects;

/// <summary>
/// Value Object для контенту відгуку
/// </summary>
[BsonSerializer(typeof(ReviewContentBsonSerializer))]
public class ReviewContent : ValueObject
{
    public string Title { get; private set; }
    public string Content { get; private set; }
    public bool IsSpoiler { get; private set; }

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

/// <summary>
/// BSON Serializer для ReviewContent
/// </summary>
public class ReviewContentBsonSerializer : IBsonSerializer<ReviewContent>
{
    public Type ValueType => typeof(ReviewContent);

    public ReviewContent Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        context.Reader.ReadStartDocument();
        
        string title = string.Empty;
        string content = string.Empty;
        bool isSpoiler = false;

        while (context.Reader.State != BsonReaderState.EndOfDocument)
        {
            var name = context.Reader.ReadName();
            
            switch (name)
            {
                case "title":
                    title = context.Reader.ReadString();
                    break;
                case "content":
                    content = context.Reader.ReadString();
                    break;
                case "isSpoiler":
                    isSpoiler = context.Reader.ReadBoolean();
                    break;
                default:
                    context.Reader.SkipValue();
                    break;
            }
        }

        context.Reader.ReadEndDocument();
        return ReviewContent.Create(title, content, isSpoiler);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ReviewContent value)
    {
        context.Writer.WriteStartDocument();
        context.Writer.WriteName("title");
        context.Writer.WriteString(value.Title);
        context.Writer.WriteName("content");
        context.Writer.WriteString(value.Content);
        context.Writer.WriteName("isSpoiler");
        context.Writer.WriteBoolean(value.IsSpoiler);
        context.Writer.WriteEndDocument();
    }

    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return Deserialize(context, args);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        if (value is ReviewContent content)
            Serialize(context, args, content);
        else
            throw new NotSupportedException($"Cannot serialize {value?.GetType()}");
    }
}