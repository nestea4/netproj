using AutoMapper;
using CatalogService.Models.DTOs;
using CatalogService.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers;

/// <summary>
/// API для управління категоріями фільмів
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryRepository categoryRepository,
        IMapper mapper,
        ILogger<CategoriesController> logger)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Отримання всіх категорій
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Список категорій</returns>
    /// <response code="200">Успішне отримання списку категорій</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCategories(CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetCategoriesWithMoviesAsync(cancellationToken);
        var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
        
        return Ok(categoryDtos);
    }

    /// <summary>
    /// Отримання категорії за slug
    /// </summary>
    /// <param name="slug">URL-friendly ідентифікатор категорії</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Інформація про категорію</returns>
    /// <response code="200">Категорія знайдена</response>
    /// <response code="404">Категорія не знайдена</response>
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryBySlug(string slug, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetBySlugAsync(slug, cancellationToken);
        
        if (category == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Category with slug '{slug}' not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        var categoryDto = _mapper.Map<CategoryDto>(category);
        return Ok(categoryDto);
    }
}