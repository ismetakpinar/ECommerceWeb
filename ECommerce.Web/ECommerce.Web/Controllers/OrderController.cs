using Microsoft.AspNetCore.Mvc;
using ECommerce.Web.Infrastructure.Authentication;

namespace ECommerce.Web.Controllers;


public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ITokenService _tokenService;

    public OrderController(
        IOrderService orderService,
        ITokenService tokenService)
    {
        _orderService = orderService;
        _tokenService = tokenService;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_tokenService.GetAccessToken()))
        {
            return RedirectToAction("Login", "Account");
        }

        var response =
            await _orderService.GetMyOrdersAsync(
                cancellationToken);

        if (response is null || !response.Success)
        {
            TempData["Error"] = response?.Message ?? "Siparişleriniz alınamadı.";
        }

        return View(response?.Data ?? []);
    }

    public async Task<IActionResult> Detail(
        int id,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_tokenService.GetAccessToken()))
        {
            return RedirectToAction("Login", "Account");
        }

        var response =
            await _orderService.GetMyOrderDetailAsync(
                id,
                cancellationToken);

        if (response is null || !response.Success || response.Data is null)
        {
            return NotFound();
        }

        return View(response.Data);
    }
}