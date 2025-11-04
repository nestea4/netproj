using CatalogService.Data;
using CatalogService.Models;
using CatalogService.Repositories;
using CatalogService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace CatalogService.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly CatalogDbContext _context;
    private IDbContextTransaction? _transaction;
    
    // Lazy initialization для репозиторіїв
    private IMovieRepository? _movieRepository;
    private IShowtimeRepository? _showtimeRepository;
    private ICategoryRepository? _categoryRepository;
    private IGenericRepository<Hall>? _hallRepository;
    private IGenericRepository<MovieDetails>? _movieDetailsRepository;
    private IGenericRepository<MovieCategory>? _movieCategoryRepository;

    public UnitOfWork(CatalogDbContext context)
    {
        _context = context;
    }

    // Lazy-ініціалізація репозиторіїв
    public IMovieRepository Movies => 
        _movieRepository ??= new MovieRepository(_context);

    public IShowtimeRepository Showtimes => 
        _showtimeRepository ??= new ShowtimeRepository(_context);

    public ICategoryRepository Categories => 
        _categoryRepository ??= new CategoryRepository(_context);

    public IGenericRepository<Hall> Halls => 
        _hallRepository ??= new GenericRepository<Hall>(_context);

    public IGenericRepository<MovieDetails> MovieDetails => 
        _movieDetailsRepository ??= new GenericRepository<MovieDetails>(_context);

    public IGenericRepository<MovieCategory> MovieCategories => 
        _movieCategoryRepository ??= new GenericRepository<MovieCategory>(_context);

    // Збереження змін
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    // Управління транзакціями
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return _transaction;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    // Dispose
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}