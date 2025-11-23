using MediatR;
using ReviewService.Application.Common;
using ReviewService.Domain.Exceptions;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Commands.Discussions;

public record AddPostCommand : ICommand<AddPostResult>
{
    public string DiscussionId { get; init; } = string.Empty;
    public long UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}

public record AddPostResult(string PostId);

public class AddPostCommandHandler : IRequestHandler<AddPostCommand, AddPostResult>
{
    private readonly IDiscussionRepository _discussionRepository;

    public AddPostCommandHandler(IDiscussionRepository discussionRepository)
    {
        _discussionRepository = discussionRepository;
    }

    public async Task<AddPostResult> Handle(AddPostCommand request, CancellationToken cancellationToken)
    {
        var discussion = await _discussionRepository.GetByIdAsync(request.DiscussionId, cancellationToken)
                         ?? throw new NotFoundException("Discussion", request.DiscussionId);

        var post = discussion.AddPost(request.UserId, request.UserName, request.Content);

        await _discussionRepository.UpdateAsync(discussion, cancellationToken);

        return new AddPostResult(post.PostId);
    }
}