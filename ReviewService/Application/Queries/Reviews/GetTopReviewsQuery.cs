using AutoMapper;
using MediatR;
using ReviewService.Application.Common;
using ReviewService.Application.Common.DTOs;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Queries.Reviews;

public record GetTopReviewsQuery : IQuery<List<ReviewDto>>
{
    public long MovieId { get; init; }
    public int Limit { get; init; } = 10;
}

public class GetTopReviewsQueryHandler : IRequestHandler<GetTopReviewsQuery, List<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetTopReviewsQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<List<ReviewDto>> Handle(GetTopReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetTopReviewsAsync(request.MovieId, request.Limit, cancellationToken);
        return _mapper.Map<List<ReviewDto>>(reviews);
    }
}