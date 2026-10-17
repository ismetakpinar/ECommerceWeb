using System.Linq.Expressions;

namespace ECommerceAPI.DataAccess.Specifications;

public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }

    List<Expression<Func<T, object>>> Includes { get; }

    Expression<Func<T, object>>? OrderBy { get; }

    Expression<Func<T, object>>? OrderByDescending { get; }

    List<string> IncludeStrings { get; }
    int Skip { get; }

    int Take { get; }

    bool IsPagingEnabled { get; }
}