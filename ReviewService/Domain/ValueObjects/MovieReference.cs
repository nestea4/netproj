using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using ReviewService.Domain.Common;
using ReviewService.Domain.Exceptions;

namespace ReviewService.Domain.ValueObjects;

/// <summary>
/// Value Object для посилання на фільм (денормалізовані дані)
/// </summary>
public class MovieReference : ValueObject
{
    [BsonElement("movieId")]
    public long MovieId { get; internal set; }
    
    [BsonElement("movieTitle")]
    public string MovieTitle { get; internal set; }

    // Parameterless constructor для BSON десеріалізації
    private MovieReference() 
    { 
        MovieTitle = string.Empty; 
    }

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