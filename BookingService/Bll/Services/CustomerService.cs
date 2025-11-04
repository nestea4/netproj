using AutoMapper;
using BookingService.Api.Controllers;
using BookingService.Bll.DTOs;
using BookingService.Bll.Interfaces;
using BookingService.Dal.Interfaces;
using BookingService.Domain.Models;

namespace BookingService.Bll.Services;

/// <summary>
///сервіс для роботи з клієнтами
///реалізує бізнес-логіку та валідацію
///BLL - єдиний власник бізнес-логіки, контролери thin
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///отримання клієнта за ID
    /// </summary>
    public async Task<CustomerDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting customer with ID: {CustomerId}", id);

        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        
        return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
    }

    /// <summary>
    ///отримання всіх клієнтів
    /// </summary>
    public async Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all customers");

        var customers = await _unitOfWork.Customers.GetAllAsync(cancellationToken);
        
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    /// <summary>
    /// Створення нового клієнта
    /// правила:
    /// - Email повинен бути унікальним
    /// - Всі обов'язкові поля повинні бути заповнені
    /// - Email повинен мати валідний формат
    /// </summary>
    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating customer with email: {Email}", request.Email);

        //валідація вхідних даних
        ValidateCustomerRequest(request);

        //правило: перевірка унікальності email
        var existingCustomer = await _unitOfWork.Customers.GetByEmailAsync(request.Email, cancellationToken);
        if (existingCustomer != null)
        {
            throw new InvalidOperationException($"Customer with email {request.Email} already exists");
        }

        //маппінг DTO в Domain model
        var customer = _mapper.Map<Customer>(request);
        
        //створення в базі даних
        var customerId = await _unitOfWork.Customers.CreateAsync(customer, cancellationToken);

        //отримання створеного клієнта
        var createdCustomer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);
        
        _logger.LogInformation("Customer created successfully with ID: {CustomerId}", customerId);

        return _mapper.Map<CustomerDto>(createdCustomer)!;
    }

    /// <summary>
    /// Оновлення даних клієнта
    ///правила:
    /// - Клієнт повинен існувати
    /// - Новий email повинен бути унікальним (якщо змінився)
    /// - Всі обов'язкові поля повинні бути заповнені
    /// </summary>
    public async Task<CustomerDto> UpdateAsync(long id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating customer with ID: {CustomerId}", id);

        //валідація вхідних даних
        ValidateCustomerRequest(request);

        // Перевірка існування клієнта
        var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (existingCustomer == null)
        {
            throw new InvalidOperationException($"Customer with ID {id} not found");
        }

        // правило: перевірка унікальності email (якщо змінився)
        if (existingCustomer.Email != request.Email)
        {
            var customerWithEmail = await _unitOfWork.Customers.GetByEmailAsync(request.Email, cancellationToken);
            if (customerWithEmail != null && customerWithEmail.CustomerId != id)
            {
                throw new InvalidOperationException($"Customer with email {request.Email} already exists");
            }
        }

        //оновлення полів
        existingCustomer.FirstName = request.FirstName;
        existingCustomer.LastName = request.LastName;
        existingCustomer.Email = request.Email;
        existingCustomer.Phone = request.Phone;
        existingCustomer.UpdatedAt = DateTime.Now;
        existingCustomer.UpdatedBy = "API";

        //збереження змін
        var updated = await _unitOfWork.Customers.UpdateAsync(existingCustomer, cancellationToken);
        
        if (!updated)
        {
            throw new InvalidOperationException($"Failed to update customer with ID {id}");
        }

        //отримання оновленого клієнта
        var updatedCustomer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        
        _logger.LogInformation("Customer updated successfully: {CustomerId}", id);

        return _mapper.Map<CustomerDto>(updatedCustomer)!;
    }

    /// <summary>
    /// Видалення клієнта (soft delete)
    /// правило: не можна видалити клієнта з активними бронюваннями
    /// </summary>
    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);

        //перевірка існування
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID {id} not found");
        }

        //правило: перевірка наявності активних бронювань
        var bookings = await _unitOfWork.Bookings.GetCustomerBookingsAsync(id, cancellationToken);
        var activeBookings = bookings.Where(b => b.Status == "Pending" || b.Status == "Confirmed").ToList();
        
        if (activeBookings.Any())
        {
            throw new InvalidOperationException(
                $"Cannot delete customer with ID {id}. Customer has {activeBookings.Count} active booking(s)");
        }
        
        var deleted = await _unitOfWork.Customers.SoftDeleteAsync(id, cancellationToken);
        
        _logger.LogInformation("Customer deleted successfully: {CustomerId}", id);

        return deleted;
    }

    /// <summary>
    /// пошук клієнта за email
    /// </summary>
    public async Task<CustomerDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting customer by email: {Email}", email);

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty", nameof(email));
        }

        var customer = await _unitOfWork.Customers.GetByEmailAsync(email, cancellationToken);
        
        return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
    }

    /// <summary>
    /// пошук клієнтів за ім'ям або прізвищем
    /// </summary>
    public async Task<IEnumerable<CustomerDto>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching customers by name: {SearchTerm}", searchTerm);

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Enumerable.Empty<CustomerDto>();
        }

        var customers = await _unitOfWork.Customers.SearchByNameAsync(searchTerm, cancellationToken);
        
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    /// <summary>
    /// Перевірка унікальності email
    /// для валідації форм на клієнті
    /// </summary>
    public async Task<bool> IsEmailUniqueAsync(string email, long? excludeCustomerId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var customer = await _unitOfWork.Customers.GetByEmailAsync(email, cancellationToken);
        
        if (customer == null)
        {
            return true;
        }

        //якщо вказано excludeCustomerId, перевіряємо чи це той самий клієнт
        return excludeCustomerId.HasValue && customer.CustomerId == excludeCustomerId.Value;
    }

    #region Private Validation Methods

    /// <summary>
    /// Валідація CreateCustomerRequest
    /// Перевіряє обов'язкові поля та формат даних
    /// </summary>
    private void ValidateCustomerRequest(CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            throw new ArgumentException("First name is required", nameof(request.FirstName));
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ArgumentException("Last name is required", nameof(request.LastName));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required", nameof(request.Email));
        }

        // Базова валідація email
        if (!request.Email.Contains("@") || !request.Email.Contains("."))
        {
            throw new ArgumentException("Invalid email format", nameof(request.Email));
        }

        // Можна додати більш складну валідацію через regex або FluentValidation
    }

    /// <summary>
    /// Валідація UpdateCustomerRequest
    /// </summary>
    private void ValidateCustomerRequest(UpdateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            throw new ArgumentException("First name is required", nameof(request.FirstName));
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ArgumentException("Last name is required", nameof(request.LastName));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required", nameof(request.Email));
        }

        if (!request.Email.Contains("@") || !request.Email.Contains("."))
        {
            throw new ArgumentException("Invalid email format", nameof(request.Email));
        }
    }

    #endregion
}