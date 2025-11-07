using AutoMapper;
using CatalogService.Exceptions;
using CatalogService.Models;
using CatalogService.Models.DTOs;
using CatalogService.Services.Interfaces;
using CatalogService.Specifications;
using CatalogService.Specifications.MovieSpecifications;
using CatalogService.UnitOfWork;

namespace CatalogService.Services;

public class MovieService : IMovieService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<MovieService> _logger;

    public MovieService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<MovieService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<MovieDto>> GetMoviesAsync(MovieFilterParameters filters, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting movies with filters: {@Filters}", filters);

        //Specification Pattern
        var specification = new MovieFilterSpecification(
            searchTerm: filters.SearchTerm,
            categoryId: filters.CategoryId,
            minRating: filters.MinRating,
            releaseDateFrom: filters.ReleaseDateFrom,
            releaseDateTo: filters.ReleaseDateTo,
            sortBy: filters.SortBy,
            sortDescending: filters.SortDescending,
            pageNumber: filters.PageNumber,
            pageSize: filters.PageSize
        );

        var movies = await _unitOfWork.Movies.GetMoviesBySpecificationAsync(specification, cancellationToken);
        var totalCount = await _unitOfWork.Movies.CountBySpecificationAsync(
            new MovieFilterSpecification(
                searchTerm: filters.SearchTerm,
                categoryId: filters.CategoryId,
                minRating: filters.MinRating,
                releaseDateFrom: filters.ReleaseDateFrom,
                releaseDateTo: filters.ReleaseDateTo
            ), 
            cancellationToken);

        var movieDtos = _mapper.Map<IEnumerable<MovieDto>>(movies);

        return new PagedResult<MovieDto>
        {
            Items = movieDtos,
            Page = filters.PageNumber,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<MovieDetailDto> GetMovieByIdAsync(long movieId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting movie with ID: {MovieId}", movieId);

        var specification = new MovieWithDetailsSpecification(movieId);
        var movies = await _unitOfWork.Movies.GetMoviesBySpecificationAsync(specification, cancellationToken);
        var movie = movies.FirstOrDefault();

        if (movie == null)
        {
            _logger.LogWarning("Movie with ID {MovieId} not found", movieId);
            throw new NotFoundException(nameof(Movie), movieId);
        }

        return _mapper.Map<MovieDetailDto>(movie);
    }

    public async Task<MovieDto> CreateMovieAsync(CreateMovieDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new movie: {Title}", dto.Title);

        //перевірка існування категорій
        foreach (var categoryId in dto.CategoryIds)
        {
            var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.CategoryId == categoryId && !c.IsDeleted, cancellationToken);
            if (!categoryExists)
            {
                throw new NotFoundException($"Category with ID {categoryId} not found");
            }
        }

        //транзакцію для забезпечення consistency
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var movie = _mapper.Map<Movie>(dto);
            movie.CreatedBy = "API";

            await _unitOfWork.Movies.AddAsync(movie, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //зв'язки M:N з категоріями
            foreach (var categoryId in dto.CategoryIds)
            {
                await _unitOfWork.MovieCategories.AddAsync(new MovieCategory
                {
                    MovieId = movie.MovieId,
                    CategoryId = categoryId
                }, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Movie created successfully with ID: {MovieId}", movie.MovieId);

            //завантажую фільм з категоріями для маппінгу
            var createdMovie = await _unitOfWork.Movies.GetMovieWithDetailsAsync(movie.MovieId, cancellationToken);
            return _mapper.Map<MovieDto>(createdMovie!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating movie");
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<MovieDto> UpdateMovieAsync(long movieId, UpdateMovieDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating movie with ID: {MovieId}", movieId);

        var movie = await _unitOfWork.Movies.GetByIdAsync(movieId, cancellationToken);

        if (movie == null || movie.IsDeleted)
        {
            throw new NotFoundException(nameof(Movie), movieId);
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            _mapper.Map(dto, movie);
            movie.UpdatedBy = "API";

            _unitOfWork.Movies.Update(movie);

            //оновити категорії
            var existingCategories = await _unitOfWork.MovieCategories
                .FindAsync(mc => mc.MovieId == movieId, cancellationToken);
            
            _unitOfWork.MovieCategories.DeleteRange(existingCategories);

            foreach (var categoryId in dto.CategoryIds)
            {
                await _unitOfWork.MovieCategories.AddAsync(new MovieCategory
                {
                    MovieId = movieId,
                    CategoryId = categoryId
                }, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Movie updated successfully: {MovieId}", movieId);

            var updatedMovie = await _unitOfWork.Movies.GetMovieWithDetailsAsync(movieId, cancellationToken);
            return _mapper.Map<MovieDto>(updatedMovie!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating movie {MovieId}", movieId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteMovieAsync(long movieId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting movie with ID: {MovieId}", movieId);

        var movie = await _unitOfWork.Movies.GetByIdAsync(movieId, cancellationToken);

        if (movie == null || movie.IsDeleted)
        {
            throw new NotFoundException(nameof(Movie), movieId);
        }

        // Soft delete
        movie.IsDeleted = true;
        _unitOfWork.Movies.Update(movie);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Movie deleted successfully: {MovieId}", movieId);
    }

    public async Task<IEnumerable<MovieDto>> GetPopularMoviesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting {Count} popular movies", count);

        var specification = new PopularMoviesSpecification(count);
        var movies = await _unitOfWork.Movies.GetMoviesBySpecificationAsync(specification, cancellationToken);

        return _mapper.Map<IEnumerable<MovieDto>>(movies);
    }

    public async Task<IEnumerable<MovieDto>> GetMoviesByCategoryAsync(long categoryId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting movies for category: {CategoryId}", categoryId);

        var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.CategoryId == categoryId && !c.IsDeleted, cancellationToken);
        if (!categoryExists)
        {
            throw new NotFoundException(nameof(Category), categoryId);
        }

        var movies = await _unitOfWork.Movies.GetMoviesByCategoryAsync(categoryId, cancellationToken);
        return _mapper.Map<IEnumerable<MovieDto>>(movies);
    }
}
