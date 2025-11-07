using AutoMapper;
using CatalogService.Models;
using CatalogService.Models.DTOs;

namespace CatalogService.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Movie mappings
        CreateMap<Movie, MovieDto>()
            .ForMember(dest => dest.Categories, 
                opt => opt.MapFrom(src => src.MovieCategories.Select(mc => mc.Category.Name)));

        CreateMap<Movie, MovieDetailDto>()
            .ForMember(dest => dest.Categories, 
                opt => opt.MapFrom(src => src.MovieCategories.Select(mc => mc.Category.Name)))
            .ForMember(dest => dest.UpcomingShowtimes,
                opt => opt.MapFrom(src => src.Showtimes
                    .Where(s => s.StartTime > DateTime.Now && !s.IsDeleted && s.IsActive)
                    .OrderBy(s => s.StartTime)
                    .Take(5)));

        CreateMap<CreateMovieDto, Movie>()
            .ForMember(dest => dest.MovieId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.MovieCategories, opt => opt.Ignore())
            .ForMember(dest => dest.Showtimes, opt => opt.Ignore())
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details));

        CreateMap<UpdateMovieDto, Movie>()
            .ForMember(dest => dest.MovieId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.MovieCategories, opt => opt.Ignore())
            .ForMember(dest => dest.Showtimes, opt => opt.Ignore())
            .ForMember(dest => dest.Details, opt => opt.Ignore());

        // MovieDetails mappings
        CreateMap<MovieDetails, MovieDetailsDto>();
        CreateMap<CreateMovieDetailsDto, MovieDetails>()
            .ForMember(dest => dest.MovieDetailsId, opt => opt.Ignore())
            .ForMember(dest => dest.MovieId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Movie, opt => opt.Ignore());

        // Showtime mappings
        CreateMap<Showtime, ShowtimeDto>()
            .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Movie.Title))
            .ForMember(dest => dest.HallName, opt => opt.MapFrom(src => src.Hall.Name));

        CreateMap<CreateShowtimeDto, Showtime>()
            .ForMember(dest => dest.ShowtimeId, opt => opt.Ignore())
            .ForMember(dest => dest.EndTime, opt => opt.Ignore())
            .ForMember(dest => dest.AvailableSeats, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Movie, opt => opt.Ignore())
            .ForMember(dest => dest.Hall, opt => opt.Ignore());

        // Category mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.MovieCount, 
                opt => opt.MapFrom(src => src.MovieCategories.Count(mc => !mc.Movie.IsDeleted)));
    }
}