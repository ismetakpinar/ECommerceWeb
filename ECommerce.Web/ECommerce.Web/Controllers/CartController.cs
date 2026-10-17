using ECommerce.Web.Services.Cart;
using ECommerce.Web.Infrastructure.Authentication;
using ECommerce.Web.ViewModels.Cart;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public sealed class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly ITokenService _tokenService;

    public CartController(ICartService cartService, ITokenService tokenService)
    {
        _cartService = cartService;
        _tokenService = tokenService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_tokenService.GetAccessToken()))
        {
            return RedirectToAction("Login", "Account");
        }

        var response = await _cartService.GetCartAsync(
            cancellationToken);

        if (response is null)
        {
            TempData["Error"] = "Sepet bilgisi alınamadı.";

            return RedirectToAction(
                "Index",
                "Home");
        }

        if (!response.Success)
        {
            TempData["Error"] = response.Message;

            return RedirectToAction(
                "Index",
                "Home");
        }

        return View(response.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(
        int productId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_tokenService.GetAccessToken()))
        {
            return RedirectToAction(
                "Login",
                "Account",
                new { returnUrl = Url.Action(nameof(Index)) });
        }

        var response = await _cartService.RemoveFromCartAsync(
            productId,
            cancellationToken);

        if (response is null)
        {
            TempData["Error"] = "İşlem gerçekleştirilemedi.";

            return RedirectToAction(nameof(Index));
        }

        TempData[response.Success ? "Success" : "Error"] =
            response.Message;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(
        int productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_tokenService.GetAccessToken()))
        {
            return RedirectToAction(
                "Login",
                "Account",
                new { returnUrl = Url.Action(nameof(Index)) });
        }

        var request = new CartUpdateViewModel
        {
            ProductId = productId,
            Quantity = quantity
        };

        var response = await _cartService.UpdateQuantityAsync(
            request,
            cancellationToken);

        if (response is null)
        {
            TempData["Error"] = "İşlem gerçekleştirilemedi.";

            return RedirectToAction(nameof(Index));
        }

        TempData[response.Success ? "Success" : "Error"] =
            response.Message;

        return RedirectToAction(nameof(Index));
    }
}