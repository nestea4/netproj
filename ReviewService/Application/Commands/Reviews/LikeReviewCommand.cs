using MediatR;
using ReviewService.Application.Common;
using ReviewService.Domain.Exceptions;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Commands.Reviews;

public record LikeReviewCommand : ICommand<bool>
{
    public string ReviewId { get; init; } = string.Empty;
    public bool IsLike { get; init; } // true = like, false = dislike
}

public class LikeReviewCommandHandler : IRequestHandler<LikeReviewCommand, bool>
{
    private readonly IReviewRepository _reviewRepository;

    public LikeReviewCommandHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<bool> Handle(LikeReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken)
                     ?? throw new NotFoundException("Review", request.ReviewId);

        if (request.IsLike)
            review.AddLike();
        else
            review.AddDislike();

        return await _reviewRepository.UpdateAsync(review, cancellationToken);
    }
}