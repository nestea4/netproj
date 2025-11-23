using AutoMapper;
using ReviewService.Application.Common.DTOs;
using ReviewService.Domain.Entities.Disscution;
using ReviewService.Domain.Entities.Review;

namespace ReviewService.Application.Common.Mappings;

public class ReviewMappingProfile : Profile
{
    public ReviewMappingProfile()
    {
        //Review - ReviewDto
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieReference.MovieId))
            .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.MovieReference.MovieTitle))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Author.UserId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Author.UserName))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.ReviewContent.Title))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.ReviewContent.Content))
            .ForMember(dest => dest.IsSpoiler, opt => opt.MapFrom(src => src.ReviewContent.IsSpoiler))
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => (int)src.Rating));

        //ReviewStatistics - ReviewStatisticsDto
        CreateMap<ReviewStatistics, ReviewStatisticsDto>();

        //Comment - CommentDto
        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Author.UserId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Author.UserName));

        //CommentReply -CommentReplyDto
        CreateMap<CommentReply, CommentReplyDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Author.UserId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Author.UserName));

        //Discussion - DiscussionDto
        CreateMap<Discussion, DiscussionDto>()
            .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieReference.MovieId))
            .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.MovieReference.MovieTitle))
            .ForMember(dest => dest.CreatorId, opt => opt.MapFrom(src => src.Creator.UserId))
            .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator.UserName));

        //DiscussionPost - DiscussionPostDto
        CreateMap<DiscussionPost, DiscussionPostDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Author.UserId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Author.UserName));
    }
}