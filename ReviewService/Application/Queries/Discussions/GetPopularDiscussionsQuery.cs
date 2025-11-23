using AutoMapper;
using MediatR;
using ReviewService.Application.Common;
using ReviewService.Application.Common.DTOs;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Queries.Discussions;

public record GetPopularDiscussionsQuery : IQuery<List<DiscussionDto>>
{
    public int Limit { get; init; } = 10;
}

public class GetPopularDiscussionsQueryHandler : IRequestHandler<GetPopularDiscussionsQuery, List<DiscussionDto>>
{
    private readonly IDiscussionRepository _discussionRepository;
    private readonly IMapper _mapper;

    public GetPopularDiscussionsQueryHandler(IDiscussionRepository discussionRepository, IMapper mapper)
    {
        _discussionRepository = discussionRepository;
        _mapper = mapper;
    }

    public async Task<List<DiscussionDto>> Handle(GetPopularDiscussionsQuery request, CancellationToken cancellationToken)
    {
        var discussions = await _discussionRepository.GetPopularDiscussionsAsync(request.Limit, cancellationToken);
        return _mapper.Map<List<DiscussionDto>>(discussions);
    }
}