using ECommerce.Web.ViewModels.Category;

namespace ECommerce.Web.ViewModels.Home;

public sealed class HomeViewModel
{
    public IReadOnlyCollection<CategoryListViewModel> Categories { get; init; }
        = [];
}