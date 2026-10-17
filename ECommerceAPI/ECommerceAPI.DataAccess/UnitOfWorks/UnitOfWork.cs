using ECommerceAPI.DataAccess.Context;
using ECommerceAPI.DataAccess.Repositories.Concrete;
using ECommerceAPI.Entity.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerceAPI.DataAccess.UnitOfWorks;

public class UnitOfWork : IUnitOfWork
{
    private readonly ECommerceDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    private IDbContextTransaction? _transaction;

    public UnitOfWork(ECommerceDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<T> Repository<T>()
        where T : BaseEntity
    {
        var type = typeof(T);

        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] =
                new GenericRepository<T>(_context);
        }

        return (IGenericRepository<T>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context
            .SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database
            .BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction
                .CommitAsync(cancellationToken);

            await _transaction.DisposeAsync();

            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction
                .RollbackAsync(cancellationToken);

            await _transaction.DisposeAsync();

            _transaction = null;
        }
    }
}