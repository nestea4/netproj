using AutoMapper;
using CatalogService.Exceptions;
using CatalogService.Models;
using CatalogService.Models.DTOs;
using CatalogService.Services.Interfaces;
using CatalogService.Specifications.ShowtimeSpecifications;
using CatalogService.UnitOfWork;

namespace CatalogService.Services;

public class ShowtimeService : IShowtimeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ShowtimeService> _logger;

    public ShowtimeService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ShowtimeService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<ShowtimeDto>> GetShowtimesAsync(long? movieId, DateTime? date, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var startDate = date?.Date;
        var endDate = date?.Date.AddDays(1);

        var specification = new ShowtimeFilterSpecification(
            movieId: movieId,
            startDate: startDate,
            endDate: endDate,
            isActive: true,
            pageNumber: pageNumber,
            pageSize: pageSize);

        var showtimes = await _unitOfWork.Showtimes.GetMoviesBySpecificationAsync(specification, cancellationToken);
        var totalCount = await _unitOfWork.Showtimes.CountBySpecificationAsync(
            new ShowtimeFilterSpecification(movieId: movieId, startDate: startDate, endDate: endDate, isActive: true),
            cancellationToken);

        var showtimeDtos = _mapper.Map<IEnumerable<ShowtimeDto>>(showtimes);

        return new PagedResult<ShowtimeDto>
        {
            Items = showtimeDtos,
            Page = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ShowtimeDto> GetShowtimeByIdAsync(long showtimeId, CancellationToken cancellationToken = default)
    {
        var showtime = await _unitOfWork.Showtimes.GetByIdAsync(showtimeId, cancellationToken);

        if (showtime == null || showtime.IsDeleted)
        {
            throw new NotFoundException(nameof(Showtime), showtimeId);
        }

        // Explicit loading
        await _unitOfWork.Showtimes.LoadMovieAsync(showtime, cancellationToken);
        await _unitOfWork.Showtimes.LoadHallAsync(showtime, cancellationToken);

        return _mapper.Map<ShowtimeDto>(showtime);
    }

    public async Task<ShowtimeDto> CreateShowtimeAsync(CreateShowtimeDto dto, CancellationToken cancellationToken = default)
    {
        //валідація фільму та залу
        var movie = await _unitOfWork.Movies.GetByIdAsync(dto.MovieId, cancellationToken);
        if (movie == null || movie.IsDeleted)
        {
            throw new NotFoundException(nameof(Movie), dto.MovieId);
        }

        var hall = await _unitOfWork.Halls.GetByIdAsync(dto.HallId, cancellationToken);
        if (hall == null || hall.IsDeleted)
        {
            throw new NotFoundException(nameof(Hall), dto.HallId);
        }

        //розрахунок EndTime
        var endTime = dto.StartTime.AddMinutes(movie.DurationMinutes);

        //перевірка доступності залу
        var isAvailable = await _unitOfWork.Showtimes.IsHallAvailableAsync(dto.HallId, dto.StartTime, endTime, cancellationToken);
        if (!isAvailable)
        {
            throw new ConflictException($"Hall {hall.Name} is not available at the specified time");
        }

        var showtime = _mapper.Map<Showtime>(dto);
        showtime.EndTime = endTime;
        showtime.AvailableSeats = hall.Capacity;

        await _unitOfWork.Showtimes.AddAsync(showtime, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Showtime created successfully with ID: {ShowtimeId}", showtime.ShowtimeId);

        return await GetShowtimeByIdAsync(showtime.ShowtimeId, cancellationToken);
    }

    public async Task DeleteShowtimeAsync(long showtimeId, CancellationToken cancellationToken = default)
    {
        var showtime = await _unitOfWork.Showtimes.GetByIdAsync(showtimeId, cancellationToken);

        if (showtime == null || showtime.IsDeleted)
        {
            throw new NotFoundException(nameof(Showtime), showtimeId);
        }

        showtime.IsDeleted = true;
        showtime.IsActive = false;
        _unitOfWork.Showtimes.Update(showtime);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Showtime deleted successfully: {ShowtimeId}", showtimeId);
    }

    public async Task<bool> UpdateSeatsAsync(long showtimeId, int seatsToReserve, CancellationToken cancellationToken = default)
    {
        var showtime = await _unitOfWork.Showtimes.GetByIdAsync(showtimeId, cancellationToken);

        if (showtime == null || showtime.IsDeleted)
        {
            throw new NotFoundException(nameof(Showtime), showtimeId);
        }

        if (showtime.AvailableSeats < seatsToReserve)
        {
            throw new BusinessException($"Not enough seats available. Available: {showtime.AvailableSeats}, Requested: {seatsToReserve}");
        }

        showtime.AvailableSeats -= seatsToReserve;
        _unitOfWork.Showtimes.Update(showtime);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}