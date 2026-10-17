using ECommerceAPI.DataAccess.Context;
using ECommerceAPI.DataAccess.Specifications;
using ECommerceAPI.Entity.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ECommerceAPI.DataAccess.Repositories.Concrete;

public class GenericRepository<T> : IGenericRepository<T>
    where T : BaseEntity
{
    private readonly ECommerceDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ECommerceDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<List<T>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .ToListAsync(cancellationToken);
    }

    public async Task<T?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                x => EF.Property<int>(
                    x,
                    $"{typeof(T).Name}Id") == id,
                cancellationToken);
    }

    public async Task<T?> GetAsync(
        Expression<Func<T, bool>> expression,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                expression,
                cancellationToken);
    }

    public async Task AddAsync(
        T entity,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(
            entity,
            cancellationToken);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void SoftDelete(T entity)
    {
        entity.IsDeleted = true;
    }

    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>> expression,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(
                expression,
                cancellationToken);
    }

    public async Task<List<T>> ListAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        var query =
            SpecificationEvaluator<T>.GetQuery(
                _dbSet.AsQueryable(),
                specification);

        return await query
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        var query =
            SpecificationEvaluator<T>.GetQuery(
                _dbSet.AsQueryable(),
                specification,
                applyPaging: false);

        return await query
            .CountAsync(cancellationToken);
    }

    public async Task<T?> GetAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        var query =
            SpecificationEvaluator<T>.GetQuery(
                _dbSet.AsQueryable(),
                specification);

        return await query
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<T>> GetAllAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        var query =
            SpecificationEvaluator<T>.GetQuery(
                _dbSet.AsQueryable(),
                specification);

        return await query
            .ToListAsync(cancellationToken);
    }
}