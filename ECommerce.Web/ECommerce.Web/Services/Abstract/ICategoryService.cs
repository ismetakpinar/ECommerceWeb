using ECommerce.Web.Models;
using ECommerce.Web.ViewModels.Category;

namespace ECommerce.Web.Services.Abstract;

public interface ICategoryService
{
    Task<ApiResponse<List<CategoryListViewModel>>?> GetAllAsync(
        CancellationToken cancellationToken = default);
}