using ECommerce.Web.Services.Abstract;
using ECommerce.Web.Services.Cart;
using ECommerce.Web.Infrastructure.Authentication;
using ECommerce.Web.Models;
using ECommerce.Web.ViewModels.Cart;
using ECommerce.Web.ViewModels.Product;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public sealed class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly ICartService _cartService;
    private readonly ITokenService _tokenService;

    public ProductController(
        IProductService productService,
        ICartService cartService,
        ITokenService tokenService)
    {
        _productService = productService;
        _cartService = cartService;
        _tokenService = tokenService;
    }

    public async Task<IActionResult> Index(
        int? categoryId,
        string? search,
        CancellationToken cancellationToken,
        int page = 1)
    {
        var request = new ProductQueryViewModel
        {
            CategoryId = categoryId,
            Search = search?.Trim(),
            Page = Math.Max(page, 1),
            PageSize = 12
        };

        ViewBag.CategoryId = categoryId;
        ViewBag.Search = request.Search;

        var response = await _productService.GetAllAsync(
            request,
            cancellationToken);

        if (response is null || !response.Success)
        {
            TempData["Error"] = response?.Message ?? "Ürünler yüklenemedi.";
            return View(new PagedResult<ProductListViewModel>
            {
                Page = request.Page,
                PageSize = request.PageSize
            });
        }

        return View(response.Data ?? new PagedResult<ProductListViewModel>());
    }

    public async Task<IActionResult> Detail(
        int id,
        CancellationToken cancellationToken)
    {
        var response = await _productService.GetByIdAsync(
            id,
            cancellationToken);

        if (response is null)
        {
            return NotFound();
        }

        if (!response.Success)
        {
            return Content(response.Message);
        }

        return View(response.Data);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(
    int productId,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_tokenService.GetAccessToken()))
        {
            var returnUrl = Url.Action(
                nameof(Detail),
                "Product",
                new { id = productId });

            return RedirectToAction(
                "Login",
                "Account",
                new { returnUrl });
        }

        var request = new CartAddViewModel
        {
            ProductId = productId,
            Quantity = 1
        };

        var response = await _cartService.AddToCartAsync(
            request,
            cancellationToken);

        if (response is null || !response.Success)
        {
            TempData["Error"] = response?.Message ?? "Ürün sepete eklenemedi.";

            return RedirectToAction(
                nameof(Detail),
                new { id = productId });
        }

        TempData["Success"] = response.Message;

        return RedirectToAction(
            nameof(Detail),
            new { id = productId });
    }
}