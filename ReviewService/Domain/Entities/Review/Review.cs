using MongoDB.Bson.Serialization.Attributes;
using ReviewService.Domain.Common;
using ReviewService.Domain.Enums;
using ReviewService.Domain.Exceptions;
using ReviewService.Domain.ValueObjects;

namespace ReviewService.Domain.Entities.Review;

/// <summary>
///агрегат для відгуку про фільм
/// </summary>
public class Review : BaseEntity
{
    [BsonElement("movieReference")]
    public MovieReference MovieReference { get; private set; }

    [BsonElement("author")]
    public UserReference Author { get; private set; }

    [BsonElement("reviewContent")]
    public ReviewContent ReviewContent { get; private set; }

    [BsonElement("rating")]
    public Rating Rating { get; private set; }

    [BsonElement("reviewType")]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public ReviewType ReviewType { get; private set; }

    [BsonElement("isVerifiedPurchase")]
    public bool IsVerifiedPurchase { get; private set; }

    [BsonElement("tags")]
    public List<string> Tags { get; private set; } = new();

    //статистика (денормалізація для швидкого читання)
    [BsonElement("statistics")]
    public ReviewStatistics Statistics { get; private set; } = new();

    //Embedded comments
    [BsonElement("comments")]
    public List<Comment> Comments { get; private set; } = new();

    //для BSON deserialization
    private Review() { }

    private Review(
        MovieReference movieReference,
        UserReference author,
        ReviewContent reviewContent,
        Rating rating,
        ReviewType reviewType,
        bool isVerifiedPurchase,
        List<string>? tags = null)
    {
        MovieReference = movieReference;
        Author = author;
        ReviewContent = reviewContent;
        Rating = rating;
        ReviewType = reviewType;
        IsVerifiedPurchase = isVerifiedPurchase;
        Tags = tags ?? new List<string>();
    }

    /// <summary>
    ///фабричний метод створення відгуку
    /// </summary>
    public static Review Create(
        long movieId,
        string movieTitle,
        long userId,
        string userName,
        string title,
        string content,
        int rating,
        bool isSpoiler = false,
        bool isVerifiedPurchase = false,
        ReviewType reviewType = ReviewType.Standard,
        List<string>? tags = null)
    {
        var movieRef = MovieReference.Create(movieId, movieTitle);
        var userRef = UserReference.Create(userId, userName);
        var reviewContent = ReviewContent.Create(title, content, isSpoiler);
        var ratingVO = Rating.Create(rating);

        return new Review(
            movieRef,
            userRef,
            reviewContent,
            ratingVO,
            reviewType,
            isVerifiedPurchase,
            tags);
    }

    /// <summary>
    /// Оновлення контенту відгуку
    /// </summary>
    public void UpdateContent(string title, string content, bool isSpoiler, List<string>? tags = null)
    {
        ReviewContent = ReviewContent.Create(title, content, isSpoiler);
        if (tags != null)
            Tags = tags;
        MarkAsModified();
    }

    /// <summary>
    /// Оновлення рейтингу
    /// </summary>
    public void UpdateRating(int rating)
    {
        Rating = Rating.Create(rating);
        MarkAsModified();
    }

    /// <summary>
    /// Додавання коментаря
    /// </summary>
    public Comment AddComment(long userId, string userName, string content)
    {
        var comment = Comment.Create(userId, userName, content);
        Comments.Add(comment);
        Statistics.IncrementCommentCount();
        MarkAsModified();
        return comment;
    }

    /// <summary>
    /// Видалення коментаря
    /// </summary>
    public void DeleteComment(string commentId)
    {
        var comment = Comments.FirstOrDefault(c => c.CommentId == commentId);
        if (comment == null)
            throw new NotFoundException("Comment", commentId);

        comment.MarkAsDeleted();
        Statistics.DecrementCommentCount();
        MarkAsModified();
    }

    /// <summary>
    /// Додавання лайка
    /// </summary>
    public void AddLike()
    {
        Statistics.IncrementLikes();
        MarkAsModified();
    }

    /// <summary>
    /// Додавання дизлайка
    /// </summary>
    public void AddDislike()
    {
        Statistics.IncrementDislikes();
        MarkAsModified();
    }

    /// <summary>
    /// Збільшення лічильника переглядів
    /// </summary>
    public void IncrementViewCount()
    {
        Statistics.IncrementViews();
        MarkAsModified();
    }

    /// <summary>
    /// Перевірка чи автор відгуку
    /// </summary>
    public bool IsAuthor(long userId) => Author.UserId == userId;
}

/// <summary>
/// Вкладена статистика відгуку
/// </summary>
[BsonNoId]
public class ReviewStatistics
{
    [BsonElement("likes")]
    public int Likes { get; private set; }

    [BsonElement("dislikes")]
    public int Dislikes { get; private set; }

    [BsonElement("viewCount")]
    public int ViewCount { get; private set; }

    [BsonElement("commentCount")]
    public int CommentCount { get; private set; }

    public void IncrementLikes() => Likes++;
    public void IncrementDislikes() => Dislikes++;
    public void IncrementViews() => ViewCount++;
    public void IncrementCommentCount() => CommentCount++;
    public void DecrementCommentCount() => CommentCount = Math.Max(0, CommentCount - 1);
}

/// <summary>
/// Вкладений коментар
/// </summary>
[BsonNoId]
public class Comment
{
    [BsonElement("commentId")]
    public string CommentId { get; private set; }

    [BsonElement("author")]
    public UserReference Author { get; private set; }

    [BsonElement("content")]
    public string Content { get; private set; }

    [BsonElement("likes")]
    public int Likes { get; private set; }

    [BsonElement("replies")]
    public List<CommentReply> Replies { get; private set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; private set; }

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; private set; }

    private Comment() { }

    private Comment(UserReference author, string content)
    {
        CommentId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        Author = author;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }

    public static Comment Create(long userId, string userName, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Comment content cannot be empty");

        if (content.Length > 1000)
            throw new DomainException("Comment content cannot exceed 1000 characters");

        var author = UserReference.Create(userId, userName);
        return new Comment(author, content);
    }

    public void AddLike() => Likes++;

    public CommentReply AddReply(long userId, string userName, string content)
    {
        var reply = CommentReply.Create(userId, userName, content);
        Replies.Add(reply);
        return reply;
    }

    public void MarkAsDeleted() => IsDeleted = true;
}

/// <summary>
/// Відповідь на коментар
/// </summary>
[BsonNoId]
public class CommentReply
{
    [BsonElement("replyId")]
    public string ReplyId { get; private set; }

    [BsonElement("author")]
    public UserReference Author { get; private set; }

    [BsonElement("content")]
    public string Content { get; private set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; private set; }

    private CommentReply() { }

    private CommentReply(UserReference author, string content)
    {
        ReplyId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        Author = author;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }

    public static CommentReply Create(long userId, string userName, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Reply content cannot be empty");

        if (content.Length > 500)
            throw new DomainException("Reply content cannot exceed 500 characters");

        var author = UserReference.Create(userId, userName);
        return new CommentReply(author, content);
    }
}