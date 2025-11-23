using MediatR;
using ReviewService.Application.Common;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Queries.Reviews;

public record GetMovieStatisticsQuery : IQuery<ReviewStatisticsDto>
{
    public long MovieId { get; init; }
}

public class GetMovieStatisticsQueryHandler : IRequestHandler<GetMovieStatisticsQuery, ReviewStatisticsDto>
{
    private readonly IReviewRepository _reviewRepository;

    public GetMovieStatisticsQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<ReviewStatisticsDto> Handle(GetMovieStatisticsQuery request, CancellationToken cancellationToken)
    {
        return await _reviewRepository.GetMovieStatisticsAsync(request.MovieId, cancellationToken);
    }
}