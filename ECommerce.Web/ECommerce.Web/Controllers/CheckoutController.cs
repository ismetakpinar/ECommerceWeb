using ECommerce.Web.Services.Abstract;
using ECommerce.Web.Services.Cart;
using ECommerce.Web.Infrastructure.Authentication;
using ECommerce.Web.ViewModels.Checkout;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public sealed class CheckoutController : Controller
{
    private readonly ICheckoutService _checkoutService;
    private readonly ICartService _cartService;
    private readonly ITokenService _tokenService;

    public CheckoutController(
        ICheckoutService checkoutService,
        ICartService cartService,
        ITokenService tokenService)
    {
        _checkoutService = checkoutService;
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

        var addressResponse =
            await _checkoutService.GetAddressesAsync(
                cancellationToken);

        var cartResponse =
            await _cartService.GetCartAsync(
                cancellationToken);

        if (addressResponse is null || cartResponse is null)
        {
            TempData["Error"] =
                "Checkout bilgileri alınamadı.";

            return RedirectToAction(
                "Index",
                "Cart");
        }

        if (!addressResponse.Success || !cartResponse.Success)
        {
            TempData["Error"] =
                addressResponse.Message
                ?? cartResponse.Message
                ?? "Checkout bilgileri alınamadı.";

            return RedirectToAction(
                "Index",
                "Cart");
        }

        if (cartResponse.Data?.Items is not { Count: > 0 })
        {
            TempData["Error"] = "Sipariş oluşturmak için sepetinizde ürün olmalı.";
            return RedirectToAction("Index", "Cart");
        }

        var model = new CheckoutViewModel
        {
            Addresses = addressResponse.Data ?? [],
            Cart = cartResponse.Data ?? new()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        CheckoutViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var addresses =
                await _checkoutService.GetAddressesAsync(
                    cancellationToken);

            var cart =
                await _cartService.GetCartAsync(
                    cancellationToken);

            model.Addresses = addresses?.Data ?? [];
            if (cart?.Success != true || cart.Data?.Items is not { Count: > 0 })
            {
                TempData["Error"] = "Sepetiniz boş veya sepet bilgileri alınamadı.";
                return RedirectToAction("Index", "Cart");
            }

            model.Cart = cart.Data;

            return View(model);
        }

        var response =
            await _checkoutService.CheckoutAsync(
                model,
                cancellationToken);

        if (response is null)
        {
            TempData["Error"] =
                "Sipariş oluşturulamadı.";

            var addresses =
                await _checkoutService.GetAddressesAsync(
                    cancellationToken);

            var cart =
                await _cartService.GetCartAsync(
                    cancellationToken);

            model.Addresses = addresses?.Data ?? [];
            if (cart?.Success != true || cart.Data?.Items is not { Count: > 0 })
            {
                TempData["Error"] = "Sepetiniz boş veya sepet bilgileri alınamadı.";
                return RedirectToAction("Index", "Cart");
            }

            model.Cart = cart.Data;

            return View(model);
        }

        if (!response.Success)
        {
            TempData["Error"] =
                response.Message;

            var addresses =
                await _checkoutService.GetAddressesAsync(
                    cancellationToken);

            var cart =
                await _cartService.GetCartAsync(
                    cancellationToken);

            model.Addresses = addresses?.Data ?? [];
            if (cart?.Success != true || cart.Data?.Items is not { Count: > 0 })
            {
                TempData["Error"] = "Sepetiniz boş veya sepet bilgileri alınamadı.";
                return RedirectToAction("Index", "Cart");
            }

            model.Cart = cart.Data;

            return View(model);
        }

        TempData["Success"] =
            "Siparişiniz başarıyla oluşturuldu.";

        return RedirectToAction(
            "Index",
            "Order");
    }
}