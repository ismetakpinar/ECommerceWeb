using ECommerce.Web.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public sealed class HomeController : Controller
{
    private readonly IProductService _productService;

    public HomeController(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var response = await _productService.GetAllAsync(
            cancellationToken);

        if (response is null)
        {
            return View();
        }

        if (!response.Success)
        {
            TempData["Error"] = response.Message;

            return View();
        }

        return View(response.Data?.Items ?? []);
    }
}