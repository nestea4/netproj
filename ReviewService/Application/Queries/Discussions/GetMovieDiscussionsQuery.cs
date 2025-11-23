using AutoMapper;
using MediatR;
using ReviewService.Application.Common;
using ReviewService.Application.Common.DTOs;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Queries.Discussions;

public record GetMovieDiscussionsQuery : IQuery<List<DiscussionDto>>
{
    public long MovieId { get; init; }
}

public class GetMovieDiscussionsQueryHandler : IRequestHandler<GetMovieDiscussionsQuery, List<DiscussionDto>>
{
    private readonly IDiscussionRepository _discussionRepository;
    private readonly IMapper _mapper;

    public GetMovieDiscussionsQueryHandler(IDiscussionRepository discussionRepository, IMapper mapper)
    {
        _discussionRepository = discussionRepository;
        _mapper = mapper;
    }

    public async Task<List<DiscussionDto>> Handle(GetMovieDiscussionsQuery request, CancellationToken cancellationToken)
    {
        var discussions = await _discussionRepository.GetByMovieIdAsync(request.MovieId, cancellationToken);
        return _mapper.Map<List<DiscussionDto>>(discussions);
    }
}