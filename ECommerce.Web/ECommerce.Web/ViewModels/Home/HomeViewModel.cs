using ECommerce.Web.ViewModels.Category;
using ECommerce.Web.ViewModels.Product;

namespace ECommerce.Web.ViewModels.Home;

public sealed class HomeViewModel
{
    public IReadOnlyCollection<ProductListViewModel> FeaturedProducts { get; init; }
        = [];

    public IReadOnlyCollection<ProductListViewModel> RecentProducts { get; init; }
        = [];

    public IReadOnlyCollection<CategoryListViewModel> Categories { get; init; }
        = [];
}