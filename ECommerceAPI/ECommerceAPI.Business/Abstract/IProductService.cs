using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Dto.Requests;
using ECommerceAPI.Entity.Entities;

namespace ECommerceAPI.Business.Abstract;

public interface IProductService
{
    Task<Product> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        Product product,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Product product,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Product>> GetPagedWithCategoryAsync(
    ProductQueryRequest request,
    CancellationToken cancellationToken = default);

    Task IncreaseStockAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken = default);

    Task DecreaseStockAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken = default);

}