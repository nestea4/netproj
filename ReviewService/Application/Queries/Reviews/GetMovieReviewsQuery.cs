using AutoMapper;
using MediatR;
using ReviewService.Application.Common;
using ReviewService.Application.Common.DTOs;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Queries.Reviews;

public record GetMovieReviewsQuery : IQuery<List<ReviewDto>>
{
    public long MovieId { get; init; }
}

public class GetMovieReviewsQueryHandler : IRequestHandler<GetMovieReviewsQuery, List<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetMovieReviewsQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<List<ReviewDto>> Handle(GetMovieReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetByMovieIdAsync(request.MovieId, cancellationToken);
        return _mapper.Map<List<ReviewDto>>(reviews);
    }
}