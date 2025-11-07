using CatalogService.Repositories.Interfaces;

namespace CatalogService.UnitOfWork;


public interface IUnitOfWork : IDisposable
{
    //репозиторії
    IMovieRepository Movies { get; }
    IShowtimeRepository Showtimes { get; }
    ICategoryRepository Categories { get; }
    IGenericRepository<Models.Hall> Halls { get; }
    IGenericRepository<Models.MovieDetails> MovieDetails { get; }
    IGenericRepository<Models.MovieCategory> MovieCategories { get; }
    
    // Операції збереження
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
    
    // Транзакції
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}