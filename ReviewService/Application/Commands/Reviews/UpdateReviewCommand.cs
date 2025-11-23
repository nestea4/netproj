using MediatR;
using ReviewService.Application.Common;
using ReviewService.Domain.Exceptions;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Commands.Reviews;

public record UpdateReviewCommand : ICommand<bool>
{
    public string ReviewId { get; init; } = string.Empty;
    public long UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool IsSpoiler { get; init; }
    public int Rating { get; init; }
    public List<string> Tags { get; init; } = new();
}

public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, bool>
{
    private readonly IReviewRepository _reviewRepository;

    public UpdateReviewCommandHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<bool> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken)
                     ?? throw new NotFoundException("Review", request.ReviewId);

        if (!review.IsAuthor(request.UserId))
            throw new BusinessRuleException("Only the author can update this review");

        review.UpdateContent(request.Title, request.Content, request.IsSpoiler, request.Tags);
        review.UpdateRating(request.Rating);

        return await _reviewRepository.UpdateAsync(review, cancellationToken);
    }
}