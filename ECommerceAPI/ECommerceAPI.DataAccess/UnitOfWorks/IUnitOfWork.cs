using ECommerceAPI.Entity.Common;

namespace ECommerceAPI.DataAccess.UnitOfWorks;

public interface IUnitOfWork
{
    IGenericRepository<T> Repository<T>()
        where T : BaseEntity;

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(
        CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(
        CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default);
}