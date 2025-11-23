using MediatR;
using ReviewService.Application.Common;
using ReviewService.Domain.Exceptions;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Commands.Reviews;

public record AddCommentCommand : ICommand<AddCommentResult>
{
    public string ReviewId { get; init; } = string.Empty;
    public long UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}

public record AddCommentResult(string CommentId);

public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, AddCommentResult>
{
    private readonly IReviewRepository _reviewRepository;

    public AddCommentCommandHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<AddCommentResult> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken)
                     ?? throw new NotFoundException("Review", request.ReviewId);

        var comment = review.AddComment(request.UserId, request.UserName, request.Content);

        await _reviewRepository.UpdateAsync(review, cancellationToken);

        return new AddCommentResult(comment.CommentId);
    }
}