using MediatR;
using ReviewService.Application.Common;
using ReviewService.Domain.Entities.Review;
using ReviewService.Domain.Enums;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Commands.Reviews;

/// <summary>
/// Команда створення відгуку
/// </summary>
public record CreateReviewCommand : ICommand<CreateReviewResult>
{
    public long MovieId { get; init; }
    public string MovieTitle { get; init; } = string.Empty;
    public long UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public int Rating { get; init; }
    public bool IsSpoiler { get; init; }
    public bool IsVerifiedPurchase { get; init; }
    public ReviewType ReviewType { get; init; } = ReviewType.Standard;
    public List<string> Tags { get; init; } = new();
}

public record CreateReviewResult(string ReviewId);

/// <summary>
/// Handler для створення відгуку
/// </summary>
public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, CreateReviewResult>
{
    private readonly IReviewRepository _reviewRepository;

    public CreateReviewCommandHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<CreateReviewResult> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        // Створення агрегату Review через доменні методи
        var review = Review.Create(
            request.MovieId,
            request.MovieTitle,
            request.UserId,
            request.UserName,
            request.Title,
            request.Content,
            request.Rating,
            request.IsSpoiler,
            request.IsVerifiedPurchase,
            request.ReviewType,
            request.Tags
        );

        // Збереження через репозиторій
        var created = await _reviewRepository.CreateAsync(review, cancellationToken);

        return new CreateReviewResult(created.Id);
    }
}