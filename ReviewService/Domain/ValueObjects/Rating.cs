using MongoDB.Bson.Serialization.Attributes;
using ReviewService.Domain.Common;
using ReviewService.Domain.Exceptions;

namespace ReviewService.Domain.ValueObjects;

/// <summary>
/// Value Object для рейтингу (1-10)
/// </summary>
[BsonSerializer(typeof(RatingBsonSerializer))]
public class Rating : ValueObject
{
    public int Value { get; private set; }

    private Rating(int value)
    {
        Value = value;
    }

    public static Rating Create(int value)
    {
        if (value < 1 || value > 10)
            throw new DomainException("Rating must be between 1 and 10");

        return new Rating(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator int(Rating rating) => rating.Value;
}

/// <summary>
/// BSON Serializer для Rating
/// </summary>
public class RatingBsonSerializer : MongoDB.Bson.Serialization.Serializers.SerializerBase<Rating>
{
    public override void Serialize(MongoDB.Bson.Serialization.BsonSerializationContext context, 
        MongoDB.Bson.Serialization.BsonSerializationArgs args, Rating value)
    {
        context.Writer.WriteInt32(value.Value);
    }

    public override Rating Deserialize(MongoDB.Bson.Serialization.BsonDeserializationContext context, 
        MongoDB.Bson.Serialization.BsonDeserializationArgs args)
    {
        var value = context.Reader.ReadInt32();
        return Rating.Create(value);
    }
}