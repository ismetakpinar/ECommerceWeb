using ECommerce.Web.Models;
using ECommerce.Web.ViewModels.Product;

namespace ECommerce.Web.Services.Abstract;

public interface IProductService
{
    Task<ApiResponse<PagedResult<ProductListViewModel>>?> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ProductDetailViewModel>?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ProductDetailViewModel>?> CreateAsync(
        CreateProductViewModel request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ProductDetailViewModel>?> UpdateAsync(
        int id,
        UpdateProductViewModel request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object>?> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}