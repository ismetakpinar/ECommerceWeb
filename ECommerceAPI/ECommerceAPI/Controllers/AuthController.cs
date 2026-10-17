using Asp.Versioning;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Dto.Dtos;
using ECommerceAPI.Dto.Dtos.AuthDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace ECommerceAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[EnableRateLimiting("GeneralPolicy")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [EnableRateLimiting("RegisterPolicy")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterDto dto,
        CancellationToken cancellationToken = default)
    {
        await _authService.RegisterAsync(
            dto,
            cancellationToken);

        return Ok(
            ApiResponse<string>.SuccessResponse(
                message: "Kullanıcı başarıyla kaydedildi."));
    }

    [EnableRateLimiting("LoginPolicy")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _authService.LoginAsync(
            dto,
            cancellationToken);

        return Ok(
            ApiResponse<LoginResponseDto>
                .SuccessResponse(
                    result,
                    "Giriş başarılı."));
    }

    [HttpGet("secret")]
    [Authorize]
    public IActionResult Secret()
    {
        return Ok(
            ApiResponse<string>
                .SuccessResponse(
                    "Token geçerli. Bu alan korumalıdır.",
                    "İşlem başarılı."));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult Admin()
    {
        return Ok(
            ApiResponse<string>
                .SuccessResponse(
                    "Sadece Admin erişebilir.",
                    "İşlem başarılı."));
    }

    [Authorize(Roles = "User")]
    [HttpGet("user")]
    public IActionResult GetCurrentUser()
    {
        return Ok(
            ApiResponse<string>
                .SuccessResponse(
                    "Sadece User erişebilir.",
                    "İşlem başarılı."));
    }
    [EnableRateLimiting("RefreshPolicy")]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _authService.RefreshTokenAsync(
            request,
            cancellationToken);

        return Ok(
            ApiResponse<LoginResponseDto>
                .SuccessResponse(
                    result,
                    "Token yenilendi."));
    }
    [Authorize]
    [EnableRateLimiting("GeneralPolicy")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await _authService.LogoutAsync(
            request,
            cancellationToken);

        return Ok(
            ApiResponse<string>.SuccessResponse(
                "Çıkış başarılı."));
    }
}