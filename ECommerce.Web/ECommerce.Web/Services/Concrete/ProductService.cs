using ECommerce.Web.Infrastructure.Http;
using ECommerce.Web.Models;
using ECommerce.Web.Services.Abstract;
using ECommerce.Web.ViewModels.Product;
using ECommerceAPI.Entity.Enums;


namespace ECommerce.Web.Services.Concrete;

public sealed class ProductService
    : BaseApiService, IProductService
{
    public ProductService(HttpClient httpClient)
        : base(httpClient)
    {
    }


    public async Task<ApiResponse<PagedResult<ProductListViewModel>>?> GetAllAsync(
    ProductQueryViewModel request,
    CancellationToken cancellationToken = default)
    {
        var query = new List<string>
    {
        $"page={request.Page}",
        $"pageSize={request.PageSize}"
    };

        if (request.CategoryId.HasValue)
            query.Add($"categoryId={request.CategoryId.Value}");

        if (request.IsFeatured.HasValue)
            query.Add($"isFeatured={request.IsFeatured.Value.ToString().ToLower()}");

        if (!string.IsNullOrWhiteSpace(request.Search))
            query.Add($"search={Uri.EscapeDataString(request.Search)}");

        if (request.SortBy != ProductSortBy.None)
            query.Add($"sortBy={(int)request.SortBy}");

        var endpoint =
            $"{ApiEndpoints.Products}?{string.Join("&", query)}";

        return await GetAsync<PagedResult<ProductListViewModel>>(
            endpoint,
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