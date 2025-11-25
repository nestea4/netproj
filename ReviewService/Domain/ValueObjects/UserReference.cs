using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using ReviewService.Domain.Common;
using ReviewService.Domain.Exceptions;

namespace ReviewService.Domain.ValueObjects;


/// <summary>
/// Value Object для автора (користувача)
/// </summary>
public class UserReference : ValueObject
{
    [BsonElement("userId")]
    public long UserId { get; internal set; }
    
    [BsonElement("userName")]
    public string UserName { get; internal set; }

    // Parameterless constructor для BSON десеріалізації
    private UserReference() 
    { 
        UserName = string.Empty; 
    }

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