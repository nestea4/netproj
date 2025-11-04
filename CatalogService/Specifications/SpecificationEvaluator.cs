using Microsoft.EntityFrameworkCore;

namespace CatalogService.Specifications;

//Відповідає за застосування специфікації до IQueryable<T>, що часто є запитом Entity Framework
public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> specification) where T : class
    {
        var query = inputQuery;

        //застосування фільтрації (Where)
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        //застосування Include-ів
        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));
        query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        //застосування сортування
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        //застосування пагінації
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }
}