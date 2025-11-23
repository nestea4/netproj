using MediatR;
using ReviewService.Application.Common;
using ReviewService.Domain.Entities.Disscution;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Commands.Discussions;

public record CreateDiscussionCommand : ICommand<CreateDiscussionResult>
{
    public long MovieId { get; init; }
    public string MovieTitle { get; init; } = string.Empty;
    public long UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record CreateDiscussionResult(string DiscussionId);

public class CreateDiscussionCommandHandler : IRequestHandler<CreateDiscussionCommand, CreateDiscussionResult>
{
    private readonly IDiscussionRepository _discussionRepository;

    public CreateDiscussionCommandHandler(IDiscussionRepository discussionRepository)
    {
        _discussionRepository = discussionRepository;
    }

    public async Task<CreateDiscussionResult> Handle(CreateDiscussionCommand request, CancellationToken cancellationToken)
    {
        var discussion = Discussion.Create(
            request.MovieId,
            request.MovieTitle,
            request.UserId,
            request.UserName,
            request.Topic,
            request.Description
        );

        var created = await _discussionRepository.CreateAsync(discussion, cancellationToken);

        return new CreateDiscussionResult(created.Id);
    }
}