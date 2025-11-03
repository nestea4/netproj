using BookingService.Bll.DTOs;
using BookingService.Bll.Interfaces;
using BookingService.Dal.Interfaces;
using BookingService.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Api.Controllers;

/// <summary>
/// API для управління клієнтами
/// Thin controller - вся бізнес-логіка в BLL
/// Демонструє правильну роботу з HTTP-статусами, асинхронністю та атрибутною маршрутизацією
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(
        ICustomerService customerService, 
        ILogger<CustomerController> logger)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Отримання всіх клієнтів
    /// </summary>
    /// <param name="cancellationToken">Токен скасування операції</param>
    /// <returns>Список клієнтів</returns>
    /// <response code="200">Успішно отримано список клієнтів</response>
    /// <response code="500">Внутрішня помилка сервера</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting all customers");
            
            var customers = await _customerService.GetAllAsync(cancellationToken);
            
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all customers");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ErrorResponse 
                { 
                    Error = "Internal server error",
                    Details = ex.Message 
                });
        }
    }

    /// <summary>
    /// Отримання клієнта за ID
    /// </summary>
    /// <param name="id">ID клієнта</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Дані клієнта</returns>
    /// <response code="200">Клієнта знайдено</response>
    /// <response code="404">Клієнта не знайдено</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting customer with ID: {CustomerId}", id);
            
            var customer = await _customerService.GetByIdAsync(id, cancellationToken);
            
            if (customer == null)
            {
                return NotFound(new ErrorResponse 
                { 
                    Error = "Customer not found",
                    Details = $"Customer with ID {id} does not exist"
                });
            }

            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer with ID: {CustomerId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ErrorResponse 
                { 
                    Error = "Internal server error",
                    Details = ex.Message 
                });
        }
    }

    /// <summary>
    /// Створення нового клієнта
    /// </summary>
    /// <param name="request">Дані для створення клієнта</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Створений клієнт</returns>
    /// <response code="201">Клієнта успішно створено (повертає Location header)</response>
    /// <response code="400">Помилка валідації вхідних даних</response>
    /// <response code="409">Конфлікт - email вже використовується</response>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerRequest request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating new customer with email: {Email}", request.Email);

            var customer = await _customerService.CreateAsync(request, cancellationToken);

            _logger.LogInformation("Customer created successfully with ID: {CustomerId}", customer.CustomerId);

            // 201 Created з Location header, який вказує на URL нового ресурсу
            return CreatedAtAction(
                nameof(GetById),
                new { id = customer.CustomerId },
                customer
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            // 409 Conflict - email вже використовується
            return Conflict(new ErrorResponse
            {
                Error = "Email conflict",
                Details = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            // 400 Bad Request - валідація не пройшла
            return BadRequest(new ErrorResponse
            {
                Error = "Validation failed",
                Details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ErrorResponse 
                { 
                    Error = "Internal server error",
                    Details = ex.Message 
                });
        }
    }

    /// <summary>
    /// Оновлення даних клієнта
    /// </summary>
    /// <param name="id">ID клієнта</param>
    /// <param name="request">Оновлені дані</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Оновлені дані клієнта</returns>
    /// <response code="200">Клієнта успішно оновлено</response>
    /// <response code="400">Помилка валідації</response>
    /// <response code="404">Клієнта не знайдено</response>
    /// <response code="409">Конфлікт - новий email вже використовується</response>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        long id, 
        [FromBody] UpdateCustomerRequest request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating customer with ID: {CustomerId}", id);

            var customer = await _customerService.UpdateAsync(id, request, cancellationToken);

            _logger.LogInformation("Customer updated successfully: {CustomerId}", id);

            return Ok(customer);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new ErrorResponse
            {
                Error = "Customer not found",
                Details = ex.Message
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new ErrorResponse
            {
                Error = "Email conflict",
                Details = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Validation failed",
                Details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ErrorResponse 
                { 
                    Error = "Internal server error",
                    Details = ex.Message 
                });
        }
    }

    /// <summary>
    /// Видалення клієнта (soft delete)
    /// </summary>
    /// <param name="id">ID клієнта</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Результат операції</returns>
    /// <response code="204">Клієнта успішно видалено (без тіла відповіді)</response>
    /// <response code="404">Клієнта не знайдено</response>
    /// <response code="409">Конфлікт - не можна видалити клієнта з активними бронюваннями</response>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);

            await _customerService.DeleteAsync(id, cancellationToken);

            _logger.LogInformation("Customer deleted successfully: {CustomerId}", id);

            // 204 No Content - успіх без тіла відповіді
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new ErrorResponse
            {
                Error = "Customer not found",
                Details = ex.Message
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("active booking"))
        {
            // 409 Conflict - не можна видалити через активні бронювання
            return Conflict(new ErrorResponse
            {
                Error = "Cannot delete customer",
                Details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ErrorResponse 
                { 
                    Error = "Internal server error",
                    Details = ex.Message 
                });
        }
    }

    /// <summary>
    /// Пошук клієнта за email
    /// </summary>
    /// <param name="email">Email для пошуку</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Дані клієнта</returns>
    /// <response code="200">Клієнта знайдено</response>
    /// <response code="404">Клієнта не знайдено</response>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByEmail(string email, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting customer by email: {Email}", email);

            var customer = await _customerService.GetByEmailAsync(email, cancellationToken);

            if (customer == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "Customer not found",
                    Details = $"Customer with email {email} does not exist"
                });
            }

            return Ok(customer);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Validation failed",
                Details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by email");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ErrorResponse 
                { 
                    Error = "Internal server error",
                    Details = ex.Message 
                });
        }
    }

    /// <summary>
    /// Пошук клієнтів за ім'ям або прізвищем
    /// </summary>
    /// <param name="searchTerm">Термін для пошуку</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Список знайдених клієнтів</returns>
    /// <response code="200">Список клієнтів (може бути порожнім)</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search(
        [FromQuery] string searchTerm, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Searching customers by: {SearchTerm}", searchTerm);

            var customers = await _customerService.SearchByNameAsync(searchTerm, cancellationToken);

            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ErrorResponse 
                { 
                    Error = "Internal server error",
                    Details = ex.Message 
                });
        }
    }

    /// <summary>
    /// Перевірка доступності email
    /// Корисно для валідації форм на клієнті в реальному часі
    /// </summary>
    /// <param name="email">Email для перевірки</param>
    /// <param name="excludeCustomerId">ID клієнта для виключення з перевірки (при оновленні)</param>
    /// <param name="cancellationToken">Токен скасування</param>
    /// <returns>Результат перевірки</returns>
    /// <response code="200">Результат перевірки (true/false)</response>
    [HttpGet("check-email")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckEmailAvailability(
        [FromQuery] string email,
        [FromQuery] long? excludeCustomerId,
        CancellationToken cancellationToken)
    {
        try
        {
            var isUnique = await _customerService.IsEmailUniqueAsync(
                email, 
                excludeCustomerId, 
                cancellationToken);

            return Ok(new { email, isAvailable = isUnique });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ErrorResponse 
                { 
                    Error = "Internal server error",
                    Details = ex.Message 
                });
        }
    }
}