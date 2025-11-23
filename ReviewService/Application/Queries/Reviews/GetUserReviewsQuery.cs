using AutoMapper;
using MediatR;
using ReviewService.Application.Common;
using ReviewService.Application.Common.DTOs;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Queries.Reviews;

public record GetUserReviewsQuery : IQuery<List<ReviewDto>>
{
    public long UserId { get; init; }
}

public class GetUserReviewsQueryHandler : IRequestHandler<GetUserReviewsQuery, List<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetUserReviewsQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<List<ReviewDto>> Handle(GetUserReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        return _mapper.Map<List<ReviewDto>>(reviews);
    }
}