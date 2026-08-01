using ECommerce.Web.Infrastructure.Http;
using ECommerce.Web.Models;
using ECommerce.Web.Services.Abstract;
using ECommerce.Web.ViewModels.Product;

namespace ECommerce.Web.Services.Concrete;

public sealed class ProductService
    : BaseApiService, IProductService
{
    public ProductService(HttpClient httpClient)
        : base(httpClient)
    {
    }

    // ...


    public async Task<ApiResponse<PagedResult<ProductListViewModel>>?> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await GetAsync<PagedResult<ProductListViewModel>>(
            ApiEndpoints.Products,
            cancellationToken);
    }

    public Task<ApiResponse<ProductDetailViewModel>?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<ProductDetailViewModel>?> CreateAsync(
        CreateProductViewModel request,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<ProductDetailViewModel>?> UpdateAsync(
        int id,
        UpdateProductViewModel request,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<object>?> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}