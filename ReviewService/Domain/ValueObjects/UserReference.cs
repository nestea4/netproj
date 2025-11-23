using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using ReviewService.Domain.Common;
using ReviewService.Domain.Exceptions;

namespace ReviewService.Domain.ValueObjects;

/// <summary>
///Value Object для автора (користувача)
/// </summary>
[BsonSerializer(typeof(UserReferenceBsonSerializer))]
public class UserReference : ValueObject
{
    public long UserId { get; private set; }
    public string UserName { get; private set; }

    private UserReference(long userId, string userName)
    {
        UserId = userId;
        UserName = userName;
    }

    public static UserReference Create(long userId, string userName)
    {
        if (userId <= 0)
            throw new DomainException("User ID must be positive");

        if (string.IsNullOrWhiteSpace(userName))
            throw new DomainException("User name cannot be empty");

        return new UserReference(userId, userName);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return UserId;
        yield return UserName;
    }
}

/// <summary>
///BSON Serializer для UserReference
/// </summary>
public class UserReferenceBsonSerializer : IBsonSerializer<UserReference>
{
    public Type ValueType => typeof(UserReference);

    public UserReference Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        context.Reader.ReadStartDocument();
        
        long userId = 0;
        string userName = string.Empty;

        while (context.Reader.State != BsonReaderState.EndOfDocument)
        {
            var name = context.Reader.ReadName();
            
            switch (name)
            {
                case "userId":
                    userId = context.Reader.ReadInt64();
                    break;
                case "userName":
                    userName = context.Reader.ReadString();
                    break;
                default:
                    context.Reader.SkipValue();
                    break;
            }
        }

        context.Reader.ReadEndDocument();
        return UserReference.Create(userId, userName);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, UserReference value)
    {
        context.Writer.WriteStartDocument();
        context.Writer.WriteName("userId");
        context.Writer.WriteInt64(value.UserId);
        context.Writer.WriteName("userName");
        context.Writer.WriteString(value.UserName);
        context.Writer.WriteEndDocument();
    }

    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return Deserialize(context, args);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        if (value is UserReference userRef)
            Serialize(context, args, userRef);
        else
            throw new NotSupportedException($"Cannot serialize {value?.GetType()}");
    }
}