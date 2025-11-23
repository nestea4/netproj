using MediatR;
using ReviewService.Application.Common;
using ReviewService.Domain.Exceptions;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Commands.Reviews;

public record DeleteReviewCommand : ICommand<bool>
{
    public string ReviewId { get; init; } = string.Empty;
    public long UserId { get; init; }
}

public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, bool>
{
    private readonly IReviewRepository _reviewRepository;

    public DeleteReviewCommandHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<bool> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken)
                     ?? throw new NotFoundException("Review", request.ReviewId);

        if (!review.IsAuthor(request.UserId))
            throw new BusinessRuleException("Only the author can delete this review");

        return await _reviewRepository.DeleteAsync(request.ReviewId, cancellationToken);
    }
}