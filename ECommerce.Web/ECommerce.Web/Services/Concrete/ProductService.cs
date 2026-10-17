using ECommerce.Web.Infrastructure.Authentication;
using ECommerce.Web.Infrastructure.Http;
using ECommerce.Web.Models;
using ECommerce.Web.Services.Abstract;
using ECommerce.Web.ViewModels.Product;
using ECommerceAPI.Entity.Enums;


namespace ECommerce.Web.Services.Concrete;

public sealed class ProductService
    : BaseApiService, IProductService
{
    public ProductService(
    HttpClient httpClient,
    ITokenService tokenService)
    : base(httpClient, tokenService)
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
            $"{ApiEndpoints.Product.Base}?{string.Join("&", query)}";

        return await GetAsync<PagedResult<ProductListViewModel>>(
            endpoint,
            cancellationToken);
    }
    public async Task<ApiResponse<ProductDetailViewModel>?> GetByIdAsync(
    int productId,
    CancellationToken cancellationToken = default)
    {
        return await GetAsync<ProductDetailViewModel>(
            $"{ApiEndpoints.Product.Base}/{productId}",
            cancellationToken);
    }

    public Task<ApiResponse<ProductDetailViewModel>?> CreateAsync(
        CreateProductViewModel request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<object, ProductDetailViewModel>(
            ApiEndpoints.Product.Base,
            new
            {
                request.Name,
                Description = request.Description ?? string.Empty,
                request.Price,
                request.Stock,
                request.CategoryId,
                ImageUrl = "/theme/img/product-1.jpg",
                ShortDescription = (string?)null,
                IsFeatured = false
            },
            cancellationToken);
    }

    public Task<ApiResponse<ProductDetailViewModel>?> UpdateAsync(
        int id,
        UpdateProductViewModel request,
        CancellationToken cancellationToken = default)
    {
        return PutAsync<object, ProductDetailViewModel>(
            $"{ApiEndpoints.Product.Base}/{id}",
            new
            {
                request.Name,
                Description = request.Description ?? string.Empty,
                request.Price,
                request.CategoryId,
                ImageUrl = "/theme/img/product-1.jpg",
                ShortDescription = (string?)null,
                IsFeatured = false
            },
            cancellationToken);
    }

    public Task<ApiResponse<object>?> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return base.DeleteAsync(
            $"{ApiEndpoints.Product.Base}/{id}",
            cancellationToken);
    }


}