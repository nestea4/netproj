using AutoMapper;
using MediatR;
using ReviewService.Application.Common;
using ReviewService.Application.Common.DTOs;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Queries.Discussions;

public record GetDiscussionByIdQuery : IQuery<DiscussionDto?>
{
    public string DiscussionId { get; init; } = string.Empty;
}

public class GetDiscussionByIdQueryHandler : IRequestHandler<GetDiscussionByIdQuery, DiscussionDto?>
{
    private readonly IDiscussionRepository _discussionRepository;
    private readonly IMapper _mapper;

    public GetDiscussionByIdQueryHandler(IDiscussionRepository discussionRepository, IMapper mapper)
    {
        _discussionRepository = discussionRepository;
        _mapper = mapper;
    }

    public async Task<DiscussionDto?> Handle(GetDiscussionByIdQuery request, CancellationToken cancellationToken)
    {
        var discussion = await _discussionRepository.GetByIdAsync(request.DiscussionId, cancellationToken);
        
        if (discussion != null)
        {
            discussion.IncrementViewCount();
            await _discussionRepository.UpdateAsync(discussion, cancellationToken);
        }

        return _mapper.Map<DiscussionDto>(discussion);
    }
}