using AutoMapper;
using MediatR;
using ReviewService.Application.Common;
using ReviewService.Application.Common.DTOs;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Queries.Reviews;

public record SearchReviewsQuery : IQuery<List<ReviewDto>>
{
    public string SearchQuery { get; init; } = string.Empty;
}

public class SearchReviewsQueryHandler : IRequestHandler<SearchReviewsQuery, List<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public SearchReviewsQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<List<ReviewDto>> Handle(SearchReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.SearchAsync(request.SearchQuery, cancellationToken);
        return _mapper.Map<List<ReviewDto>>(reviews);
    }
}