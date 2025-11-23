using ReviewService.Domain.Entities;
using ReviewService.Domain.Entities.Disscution;

namespace ReviewService.Domain.Interfaces;

/// <summary>
///репозиторій для обговорень
/// </summary>
public interface IDiscussionRepository : IRepository<Discussion>
{
    Task<List<Discussion>> GetByMovieIdAsync(long movieId, CancellationToken cancellationToken = default);
    Task<List<Discussion>> GetPopularDiscussionsAsync(int limit, CancellationToken cancellationToken = default);
    Task<List<Discussion>> GetPinnedDiscussionsAsync(CancellationToken cancellationToken = default);
}