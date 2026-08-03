using ECommerce.Web.Infrastructure.Http;
using ECommerce.Web.Models;
using ECommerce.Web.Services.Abstract;
using ECommerce.Web.ViewModels.Category;

namespace ECommerce.Web.Services.Concrete;

public sealed class CategoryService : BaseApiService, ICategoryService
{
    public CategoryService(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<ApiResponse<List<CategoryListViewModel>>?> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<CategoryListViewModel>>(
            ApiEndpoints.Categories,
            cancellationToken);
    }
}