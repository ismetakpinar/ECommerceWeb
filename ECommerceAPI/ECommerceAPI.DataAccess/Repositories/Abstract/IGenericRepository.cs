using ECommerceAPI.DataAccess.Specifications;
using ECommerceAPI.Entity.Common;
using System.Linq.Expressions;

public interface IGenericRepository<T>
    where T : BaseEntity
{
    Task<List<T>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<T?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<T?> GetAsync(
        Expression<Func<T, bool>> expression,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        T entity,
        CancellationToken cancellationToken = default);

    void Update(T entity);

    void SoftDelete(T entity);

    Task<bool> AnyAsync(
        Expression<Func<T, bool>> expression,
        CancellationToken cancellationToken = default);

    Task<List<T>> ListAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    Task<T?> GetAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    Task<List<T>> GetAllAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);
}