using System.Linq.Expressions;

namespace CatalogService.Specifications;

public interface ISpecification<T>
{
    //фільтрація
    Expression<Func<T, bool>>? Criteria { get; }
    
    //Include-и для Eager Loading
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    
    //сортування
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    
    //пагінація
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
}