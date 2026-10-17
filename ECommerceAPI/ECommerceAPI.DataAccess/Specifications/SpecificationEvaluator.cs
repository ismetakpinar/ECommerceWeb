using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.DataAccess.Specifications;

public static class SpecificationEvaluator<T>
    where T : class
{
    public static IQueryable<T> GetQuery(
    IQueryable<T> inputQuery,
    ISpecification<T> specification,
    bool applyPaging = true)
    {
        var query = inputQuery;

        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        query = specification.Includes.Aggregate(
            query,
            (current, include) => current.Include(include));

        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }

        if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(
                specification.OrderByDescending);
        }

        if (applyPaging && specification.IsPagingEnabled)
        {
            query = query
                .Skip(specification.Skip)
                .Take(specification.Take);
        }

        query = specification.IncludeStrings.Aggregate(
        query,
        (current, include) => current.Include(include));

        return query;
    }
}