using ECommerce.Web.Infrastructure.Authentication;
using ECommerce.Web.Services.Auth;
using ECommerce.Web.ViewModels.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public sealed class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public AccountController(
        IAuthService authService,
        ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var response = await _authService.LoginAsync(
            model,
            cancellationToken);

        if (response is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Sunucuya ulaşılamadı.");

            return View(model);
        }

        if (!response.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                response.Message);

            return View(model);
        }

        if (response.Data is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Giriş yanıtı alınamadı.");

            return View(model);
        }

        await _tokenService.SetTokensAsync(
            response.Data.AccessToken,
            response.Data.RefreshToken);

        TempData["Success"] = response.Message;

        if (Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl!);
        }

        return RedirectToAction(
            "Index",
            "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var response = await _authService.RegisterAsync(
            model,
            cancellationToken);

        if (response is null || !response.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                response?.Message ?? "Kayıt işlemi tamamlanamadı.");

            return View(model);
        }

        TempData["Success"] = response.Message;
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        try
        {
            var refreshToken = _tokenService.GetRefreshToken();
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await _authService.LogoutAsync(
                    refreshToken,
                    cancellationToken);
            }
        }
        catch (HttpRequestException)
        {
            TempData["Error"] =
                "Sunucuya ulaşılamadı; bu cihazdaki oturum kapatıldı.";
        }
        finally
        {
            _tokenService.RemoveTokens();
        }

        TempData["Success"] ??= "Oturumunuz kapatıldı.";
        return RedirectToAction("Index", "Home");
    }
}