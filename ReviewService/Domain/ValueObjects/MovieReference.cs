using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using ReviewService.Domain.Common;
using ReviewService.Domain.Exceptions;

namespace ReviewService.Domain.ValueObjects;

/// <summary>
/// Value Object для посилання на фільм (денормалізовані дані)
/// </summary>
[BsonSerializer(typeof(MovieReferenceBsonSerializer))]
public class MovieReference : ValueObject
{
    public long MovieId { get; private set; }
    public string MovieTitle { get; private set; }

    private MovieReference(long movieId, string movieTitle)
    {
        MovieId = movieId;
        MovieTitle = movieTitle;
    }

    public static MovieReference Create(long movieId, string movieTitle)
    {
        if (movieId <= 0)
            throw new DomainException("Movie ID must be positive");

        if (string.IsNullOrWhiteSpace(movieTitle))
            throw new DomainException("Movie title cannot be empty");

        return new MovieReference(movieId, movieTitle);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MovieId;
        yield return MovieTitle;
    }
}

/// <summary>
/// BSON Serializer для MovieReference
/// </summary>
public class MovieReferenceBsonSerializer : IBsonSerializer<MovieReference>
{
    public Type ValueType => typeof(MovieReference);

    public MovieReference Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        context.Reader.ReadStartDocument();
        
        long movieId = 0;
        string movieTitle = string.Empty;

        while (context.Reader.State != BsonReaderState.EndOfDocument)
        {
            var name = context.Reader.ReadName();
            
            switch (name)
            {
                case "movieId":
                    movieId = context.Reader.ReadInt64();
                    break;
                case "movieTitle":
                    movieTitle = context.Reader.ReadString();
                    break;
                default:
                    context.Reader.SkipValue();
                    break;
            }
        }

        context.Reader.ReadEndDocument();
        return MovieReference.Create(movieId, movieTitle);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, MovieReference value)
    {
        context.Writer.WriteStartDocument();
        context.Writer.WriteName("movieId");
        context.Writer.WriteInt64(value.MovieId);
        context.Writer.WriteName("movieTitle");
        context.Writer.WriteString(value.MovieTitle);
        context.Writer.WriteEndDocument();
    }

    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return Deserialize(context, args);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        if (value is MovieReference movieRef)
            Serialize(context, args, movieRef);
        else
            throw new NotSupportedException($"Cannot serialize {value?.GetType()}");
    }
}