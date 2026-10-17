using Asp.Versioning;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Dto.Dtos.CartDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerceAPI.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[EnableRateLimiting("GeneralPolicy")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart(
        AddToCartDto dto,
        CancellationToken cancellationToken = default)
    {
        await _cartService.AddToCartAsync(
            dto,
            cancellationToken);

        return Ok(
            ApiResponse<object>.SuccessResponse(
                null,
                "Ürün sepete eklendi."));
    }

    [HttpGet]
    public async Task<IActionResult> GetMyCart(
        CancellationToken cancellationToken = default)
    {
        var cart = await _cartService.GetMyCartAsync(
            cancellationToken);

        return Ok(
            ApiResponse<CartDto>.SuccessResponse(
                cart,
                "Sepet başarıyla getirildi."));
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateCartItem(
        UpdateCartItemDto dto,
        CancellationToken cancellationToken = default)
    {
        await _cartService.UpdateCartItemAsync(
            dto,
            cancellationToken);

        return Ok(
            ApiResponse<object>.SuccessResponse(
                null,
                "Sepet güncellendi."));
    }

    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveCartItem(
        int productId,
        CancellationToken cancellationToken = default)
    {
        await _cartService.RemoveCartItemAsync(
            productId,
            cancellationToken);

        return Ok(
            ApiResponse<object>.SuccessResponse(
                null,
                "Ürün sepetten kaldırıldı."));
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(
        CheckoutDto dto,
        CancellationToken cancellationToken = default)
    {
        await _cartService.CheckoutAsync(
            dto,
            cancellationToken);

        return Ok(
            ApiResponse<object>.SuccessResponse(
                null,
                "Sipariş başarıyla oluşturuldu."));
    }
}