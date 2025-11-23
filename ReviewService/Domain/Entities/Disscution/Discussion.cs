using MongoDB.Bson.Serialization.Attributes;
using ReviewService.Domain.Common;
using ReviewService.Domain.Exceptions;
using ReviewService.Domain.ValueObjects;

namespace ReviewService.Domain.Entities.Disscution;

/// <summary>
///агрегат для обговорення фільму
/// </summary>
public class Discussion : BaseEntity
{
    [BsonElement("movieReference")]
    public MovieReference MovieReference { get; private set; }

    [BsonElement("creator")]
    public UserReference Creator { get; private set; }

    [BsonElement("topic")]
    public string Topic { get; private set; }

    [BsonElement("description")]
    public string Description { get; private set; }

    [BsonElement("posts")]
    public List<DiscussionPost> Posts { get; private set; } = new();

    [BsonElement("participantCount")]
    public int ParticipantCount { get; private set; }

    [BsonElement("viewCount")]
    public int ViewCount { get; private set; }

    [BsonElement("isPinned")]
    public bool IsPinned { get; private set; }

    [BsonElement("isLocked")]
    public bool IsLocked { get; private set; }

    //для BSON deserialization
    private Discussion() { }

    private Discussion(
        MovieReference movieReference,
        UserReference creator,
        string topic,
        string description)
    {
        MovieReference = movieReference;
        Creator = creator;
        Topic = topic;
        Description = description;
    }

    /// <summary>
    /// Створення обговорення
    /// </summary>
    public static Discussion Create(
        long movieId,
        string movieTitle,
        long userId,
        string userName,
        string topic,
        string description)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new DomainException("Discussion topic cannot be empty");

        if (topic.Length > 200)
            throw new DomainException("Discussion topic cannot exceed 200 characters");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Discussion description cannot be empty");

        if (description.Length > 1000)
            throw new DomainException("Discussion description cannot exceed 1000 characters");

        var movieRef = MovieReference.Create(movieId, movieTitle);
        var creator = UserReference.Create(userId, userName);

        return new Discussion(movieRef, creator, topic, description);
    }

    /// <summary>
    /// Додавання поста
    /// </summary>
    public DiscussionPost AddPost(long userId, string userName, string content)
    {
        if (IsLocked)
            throw new BusinessRuleException("Cannot add posts to a locked discussion");

        var post = DiscussionPost.Create(userId, userName, content);
        Posts.Add(post);
        ParticipantCount++;
        MarkAsModified();
        return post;
    }

    /// <summary>
    /// Закріплення обговорення
    /// </summary>
    public void Pin()
    {
        IsPinned = true;
        MarkAsModified();
    }

    /// <summary>
    /// Відкріплення обговорення
    /// </summary>
    public void Unpin()
    {
        IsPinned = false;
        MarkAsModified();
    }

    /// <summary>
    /// Блокування обговорення
    /// </summary>
    public void Lock()
    {
        IsLocked = true;
        MarkAsModified();
    }

    /// <summary>
    /// Розблокування обговорення
    /// </summary>
    public void Unlock()
    {
        IsLocked = false;
        MarkAsModified();
    }

    /// <summary>
    /// Збільшення лічильника переглядів
    /// </summary>
    public void IncrementViewCount()
    {
        ViewCount++;
        MarkAsModified();
    }

    /// <summary>
    /// Перевірка чи створювач обговорення
    /// </summary>
    public bool IsCreator(long userId) => Creator.UserId == userId;
}

/// <summary>
/// Пост в обговоренні
/// </summary>
[BsonNoId]
public class DiscussionPost
{
    [BsonElement("postId")]
    public string PostId { get; private set; }

    [BsonElement("author")]
    public UserReference Author { get; private set; }

    [BsonElement("content")]
    public string Content { get; private set; }

    [BsonElement("likes")]
    public int Likes { get; private set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; private set; }

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; private set; }

    private DiscussionPost() { }

    private DiscussionPost(UserReference author, string content)
    {
        PostId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        Author = author;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }

    public static DiscussionPost Create(long userId, string userName, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Post content cannot be empty");

        if (content.Length < 10)
            throw new DomainException("Post content must be at least 10 characters");

        if (content.Length > 2000)
            throw new DomainException("Post content cannot exceed 2000 characters");

        var author = UserReference.Create(userId, userName);
        return new DiscussionPost(author, content);
    }

    public void AddLike() => Likes++;

    public void MarkAsDeleted() => IsDeleted = true;
}