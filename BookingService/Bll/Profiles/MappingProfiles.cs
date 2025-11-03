using AutoMapper;
using BookingService.Bll.DTOs;
using BookingService.Domain.Models;

namespace BookingService.Bll.Profiles;

/// <summary>
/// Профілі маппінгу для AutoMapper
/// Централізована конфігурація перетворень між Domain моделями та DTO
/// Важливо: всі маппінги повинні бути протестовані через AssertConfigurationIsValid()
/// </summary>
public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        ConfigureCustomerMappings();
        ConfigureBookingMappings();
        ConfigureTicketMappings();
    }

    /// <summary>
    /// Маппінги для Customer
    /// Domain Model <-> DTO перетворення
    /// </summary>
    private void ConfigureCustomerMappings()
    {
        // Customer -> CustomerDto (для відображення)
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.FullName, 
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

        // CreateCustomerRequest -> Customer (для створення)
        CreateMap<CreateCustomerRequest, Customer>()
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => "API"))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => "API"))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

        // UpdateCustomerRequest -> Customer (для оновлення)
        CreateMap<UpdateCustomerRequest, Customer>()
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => "API"))
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
    }

    /// <summary>
    /// Маппінги для Booking
    /// Включає маппінг складних об'єктів та computed properties
    /// </summary>
    private void ConfigureBookingMappings()
    {
        // Booking -> BookingDto (спрощене відображення для списків)
        CreateMap<Booking, BookingDto>()
            .ForMember(dest => dest.TicketCount, 
                opt => opt.MapFrom(src => src.Tickets.Count));

        // Booking -> BookingDetailsResponse (повне відображення з деталями)
        // Зверніть увагу: BookingDetailsResponse формується в репозиторії через Dapper multi-mapping
        // тому цей маппінг може не використовуватись, але залишаємо для консистентності

        // BookingStatusHistory -> BookingStatusHistoryDto
        CreateMap<BookingStatusHistory, BookingStatusHistoryDto>();

        // BookingDetails -> можна додати маппінг якщо потрібно
        CreateMap<BookingDetails, BookingDetailsResponse>()
            .ForMember(dest => dest.BookingId, opt => opt.Ignore())
            .ForMember(dest => dest.BookingNumber, opt => opt.Ignore())
            .ForMember(dest => dest.BookingDate, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentMethod, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerName, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.Phone, opt => opt.Ignore())
            .ForMember(dest => dest.FinalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Tickets, opt => opt.Ignore());
    }

    /// <summary>
    /// Маппінги для Ticket
    /// Демонструє маппінг computed properties та rename полів
    /// </summary>
    private void ConfigureTicketMappings()
    {
        // Ticket -> TicketInfo (для відображення в бронюванні)
        CreateMap<Ticket, TicketInfo>()
            .ForMember(dest => dest.Seat, 
                opt => opt.MapFrom(src => src.FullSeat)) // Computed property
            .ForMember(dest => dest.Price, 
                opt => opt.MapFrom(src => src.TicketPrice)) // Rename
            .ForMember(dest => dest.Type, 
                opt => opt.MapFrom(src => src.TicketType)); // Rename

        // AddTicketRequest -> Ticket (для створення квитка)
        CreateMap<AddTicketRequest, Ticket>()
            .ForMember(dest => dest.TicketId, opt => opt.Ignore())
            .ForMember(dest => dest.BookingId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => "API"))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));
    }
}