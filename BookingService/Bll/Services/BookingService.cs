using System.Data;
using AutoMapper;
using BookingService.Bll.DTOs;
using BookingService.Bll.Interfaces;
using BookingService.Dal.Interfaces;

namespace BookingService.Bll.Services;

/// <summary>
/// Сервіс для роботи з бронюваннями
/// Координує складні транзакційні операції через Unit of Work
/// Демонструє використання UoW для атомарності операцій
/// </summary>
public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<BookingService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BookingDetailsResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting booking with ID: {BookingId}", id);

        var booking = await _unitOfWork.Bookings.GetBookingWithTicketsAsync(id, cancellationToken);
        
        return booking;
    }

    public async Task<BookingDetailsResponse?> GetByNumberAsync(string bookingNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting booking by number: {BookingNumber}", bookingNumber);

        var booking = await _unitOfWork.Bookings.GetBookingByNumberAsync(bookingNumber, cancellationToken);
        
        if (booking == null)
        {
            return null;
        }

        return await _unitOfWork.Bookings.GetBookingWithTicketsAsync(booking.BookingId, cancellationToken);
    }

    public async Task<IEnumerable<BookingDto>> GetCustomerBookingsAsync(long customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting bookings for customer: {CustomerId}", customerId);

        var bookings = await _unitOfWork.Bookings.GetCustomerBookingsAsync(customerId, cancellationToken);
        
        return _mapper.Map<IEnumerable<BookingDto>>(bookings);
    }

    /// <summary>
    /// Створення бронювання з квитками в транзакції
    /// Демонструє використання UoW для атомарності операцій:
    /// 1. Перевірка клієнта
    /// 2. Перевірка доступності місць
    /// 3. Створення бронювання
    /// 4. Додавання квитків
    /// Всі операції виконуються в одній транзакції - або всі успішні, або rollback
    /// </summary>
    public async Task<CreateBookingResponse> CreateBookingWithTicketsAsync(
        CreateBookingWithTicketsRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating booking with tickets for customer {CustomerId}", request.CustomerId);

        // Валідація запиту
        ValidateCreateBookingRequest(request);

        try
        {
            // Початок транзакції з рівнем ізоляції ReadCommitted
            // Для OLTP операцій (створення бронювань) це оптимальний баланс
            // між консистентністю даних та performance
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            // 1. Перевірка існування клієнта
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null)
            {
                throw new InvalidOperationException($"Customer with ID {request.CustomerId} not found");
            }

            // 2. Розрахунок загальної суми
            decimal totalAmount = request.Tickets.Sum(t => t.TicketPrice);

            // 3. Перевірка доступності місць (бізнес-логіка)
            foreach (var ticket in request.Tickets)
            {
                var isAvailable = await _unitOfWork.Tickets.IsSeatAvailableAsync(
                    ticket.ShowtimeId, 
                    ticket.SeatRow, 
                    ticket.SeatNumber, 
                    cancellationToken);

                if (!isAvailable)
                {
                    throw new InvalidOperationException(
                        $"Seat {ticket.SeatRow}{ticket.SeatNumber} for showtime {ticket.ShowtimeId} is already taken");
                }
            }

            // 4. Створення бронювання через збережувану процедуру
            var (bookingId, bookingNumber) = await _unitOfWork.Bookings.CreateBookingAsync(
                request.CustomerId, 
                totalAmount, 
                "API", 
                cancellationToken);

            // 5. Додавання квитків до бронювання
            foreach (var ticket in request.Tickets)
            {
                await _unitOfWork.Bookings.AddTicketAsync(
                    bookingId, 
                    ticket, 
                    "API", 
                    cancellationToken);
            }

            // Commit транзакції - всі операції успішні
            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Booking created successfully: {BookingNumber}", bookingNumber);

            // 6. Отримання повної інформації про створене бронювання
            var bookingDetails = await _unitOfWork.Bookings.GetBookingWithTicketsAsync(bookingId, cancellationToken);

            return new CreateBookingResponse
            {
                BookingId = bookingId,
                BookingNumber = bookingNumber,
                BookingDate = DateTime.Now,
                TotalAmount = totalAmount,
                Status = "Pending",
                Tickets = bookingDetails?.Tickets ?? new List<TicketInfo>()
            };
        }
        catch (Exception ex)
        {
            // Rollback при будь-якій помилці
            if (_unitOfWork.HasActiveTransaction)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
            }
            
            _logger.LogError(ex, "Error creating booking with tickets");
            throw;
        }
    }

    public async Task ConfirmBookingAsync(long bookingId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Confirming booking {BookingId}", bookingId);

        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);
        if (booking == null)
        {
            throw new InvalidOperationException($"Booking with ID {bookingId} not found");
        }

        if (booking.Status != "Pending")
        {
            throw new InvalidOperationException($"Cannot confirm booking in status {booking.Status}");
        }

        await _unitOfWork.Bookings.ConfirmBookingAsync(bookingId, "API", cancellationToken);
    }

    public async Task PayBookingAsync(long bookingId, string paymentMethod, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Paying booking {BookingId}", bookingId);

        if (string.IsNullOrWhiteSpace(paymentMethod))
        {
            throw new ArgumentException("Payment method is required", nameof(paymentMethod));
        }

        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);
        if (booking == null)
        {
            throw new InvalidOperationException($"Booking with ID {bookingId} not found");
        }

        if (booking.Status != "Confirmed")
        {
            throw new InvalidOperationException($"Cannot pay booking in status {booking.Status}");
        }

        await _unitOfWork.Bookings.PayBookingAsync(bookingId, paymentMethod, "API", cancellationToken);
    }

    /// <summary>
    /// Підтвердження та оплата в одній транзакції
    /// Демонструє складну бізнес-логіку з транзакційністю
    /// Використовується RepeatableRead для критичних операцій оплати
    /// </summary>
    public async Task ConfirmAndPayAsync(long bookingId, string paymentMethod, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Confirming and paying booking {BookingId}", bookingId);

        if (string.IsNullOrWhiteSpace(paymentMethod))
        {
            throw new ArgumentException("Payment method is required", nameof(paymentMethod));
        }

        try
        {
            // Використовуємо RepeatableRead для критичних операцій оплати
            // Захист від non-repeatable reads під час валідації та оплати
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);

            // Перевірка існування та статусу
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                throw new InvalidOperationException($"Booking with ID {bookingId} not found");
            }

            if (booking.Status != "Pending")
            {
                throw new InvalidOperationException($"Cannot confirm and pay booking in status {booking.Status}");
            }

            // Підтвердження
            await _unitOfWork.Bookings.ConfirmBookingAsync(bookingId, "API", cancellationToken);

            // Оплата
            await _unitOfWork.Bookings.PayBookingAsync(bookingId, paymentMethod, "API", cancellationToken);

            // Commit обох операцій
            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Booking confirmed and paid successfully: {BookingId}", bookingId);
        }
        catch (Exception ex)
        {
            if (_unitOfWork.HasActiveTransaction)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
            }
            
            _logger.LogError(ex, "Error confirming and paying booking");
            throw;
        }
    }

    public async Task CancelBookingAsync(long bookingId, string reason, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling booking {BookingId}", bookingId);

        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, cancellationToken);
        if (booking == null)
        {
            throw new InvalidOperationException($"Booking with ID {bookingId} not found");
        }

        if (booking.Status == "Cancelled")
        {
            throw new InvalidOperationException("Booking is already cancelled");
        }

        if (booking.Status == "Paid")
        {
            throw new InvalidOperationException("Cannot cancel paid booking. Please contact support for refund");
        }

        await _unitOfWork.Bookings.CancelBookingAsync(bookingId, reason, "API", cancellationToken);
    }

    public async Task<IEnumerable<BookingStatusHistoryDto>> GetBookingHistoryAsync(long bookingId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting history for booking {BookingId}", bookingId);

        var history = await _unitOfWork.Bookings.GetBookingHistoryAsync(bookingId, cancellationToken);
        
        return _mapper.Map<IEnumerable<BookingStatusHistoryDto>>(history);
    }

    public async Task<IEnumerable<BookingStatisticsDto>> GetStatisticsAsync(
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting booking statistics from {FromDate} to {ToDate}", fromDate, toDate);

        // Для статистики можна використовувати окремі запити або агрегації
        // Тут спрощений варіант - в реальному проекті краще використовувати Views або складні запити
        var allBookings = await _unitOfWork.Bookings.GetAllAsync(cancellationToken);
        
        var bookingsInRange = allBookings
            .Where(b => b.BookingDate >= fromDate && b.BookingDate <= toDate)
            .ToList();

        var statistics = bookingsInRange
            .GroupBy(b => b.BookingDate.Date)
            .Select(g => new BookingStatisticsDto
            {
                Date = g.Key,
                TotalBookings = g.Count(),
                ConfirmedBookings = g.Count(b => b.Status == "Confirmed"),
                PaidBookings = g.Count(b => b.Status == "Paid"),
                CancelledBookings = g.Count(b => b.Status == "Cancelled"),
                TotalRevenue = g.Where(b => b.Status == "Paid").Sum(b => b.TotalAmount)
            })
            .OrderBy(s => s.Date)
            .ToList();

        return statistics;
    }

    /// <summary>
    /// Валідація запиту на створення бронювання
    /// Бізнес-правила та базова валідація
    /// </summary>
    private void ValidateCreateBookingRequest(CreateBookingWithTicketsRequest request)
    {
        if (request.CustomerId <= 0)
        {
            throw new ArgumentException("Invalid customer ID", nameof(request.CustomerId));
        }

        if (request.Tickets == null || !request.Tickets.Any())
        {
            throw new ArgumentException("At least one ticket is required", nameof(request.Tickets));
        }

        foreach (var ticket in request.Tickets)
        {
            if (ticket.ShowtimeId <= 0)
            {
                throw new ArgumentException("Invalid showtime ID", nameof(ticket.ShowtimeId));
            }

            if (string.IsNullOrWhiteSpace(ticket.MovieTitle))
            {
                throw new ArgumentException("Movie title is required", nameof(ticket.MovieTitle));
            }

            if (string.IsNullOrWhiteSpace(ticket.SeatRow))
            {
                throw new ArgumentException("Seat row is required", nameof(ticket.SeatRow));
            }

            if (string.IsNullOrWhiteSpace(ticket.SeatNumber))
            {
                throw new ArgumentException("Seat number is required", nameof(ticket.SeatNumber));
            }

            if (ticket.TicketPrice <= 0)
            {
                throw new ArgumentException("Ticket price must be greater than zero", nameof(ticket.TicketPrice));
            }
        }
    }
}