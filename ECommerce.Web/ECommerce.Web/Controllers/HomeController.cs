
using ECommerce.Web.Services.Abstract;
using ECommerce.Web.ViewModels.Home;
using ECommerce.Web.ViewModels.Product;
using ECommerceAPI.Entity.Enums;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public HomeController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }
    public async Task<IActionResult> Index(
    CancellationToken cancellationToken)
    {
        var request = new ProductQueryViewModel
        {
            IsFeatured = true,
            Page = 1,
            PageSize = 8
        };
        var recentRequest = new ProductQueryViewModel
        {
            Page = 1,
            PageSize = 8,
            SortBy = ProductSortBy.CreatedDateDesc
        };

        var response = await _productService.GetAllAsync(
            request,
            cancellationToken);

        var recentResponse = await _productService.GetAllAsync(
            recentRequest,
            cancellationToken);

        var categoryResponse = await _categoryService.GetAllAsync(
            cancellationToken);

        if (response is null)
        {
            return View(new HomeViewModel());
        }

        if (!response.Success)
        {
            TempData["Error"] = response.Message;

            return View(new HomeViewModel());
        }

        var model = new HomeViewModel
        {
            FeaturedProducts = response.Data?.Items ?? [],
            RecentProducts = recentResponse?.Data?.Items ?? [],
            Categories = categoryResponse?.Data ?? []
        };

        return View(model);
    }
}