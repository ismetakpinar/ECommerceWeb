using ECommerce.Web.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public sealed class ProductController : Controller
{
    private readonly IProductService _productService;

    public ProductController(
        IProductService productService)
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

