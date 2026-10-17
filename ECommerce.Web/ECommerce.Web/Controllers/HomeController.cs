
using ECommerce.Web.Services.Abstract;
using ECommerce.Web.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly ICategoryService _categoryService;

    public HomeController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    public async Task<IActionResult> Index(
    CancellationToken cancellationToken)
    {
        var categoryResponse = await _categoryService.GetAllAsync(
            cancellationToken);

        if (categoryResponse is null || !categoryResponse.Success)
        {
            TempData["Error"] = categoryResponse?.Message;

            return View(new HomeViewModel());
        }

        var model = new HomeViewModel
        {
            Categories = categoryResponse?.Data ?? []
        };

        return View(model);
    }
}