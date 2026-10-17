using ECommerce.Web.Infrastructure.Authentication;
using ECommerce.Web.Infrastructure.Http;
using ECommerce.Web.Models;
using ECommerce.Web.ViewModels.Cart;

namespace ECommerce.Web.Services.Cart;

public sealed class CartService : BaseApiService, ICartService
{
    public CartService(
        HttpClient httpClient,
        ITokenService tokenService)
        : base(httpClient, tokenService)
    {
    }

    public async Task<ApiResponse<CartViewModel>?> GetCartAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<CartViewModel>(
            ApiEndpoints.Cart.Get,
            cancellationToken);
    }

    public async Task<ApiResponse<object>?> AddToCartAsync(
        CartAddViewModel request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<CartAddViewModel, object>(
            ApiEndpoints.Cart.Add,
            request,
            cancellationToken);
    }

    public async Task<ApiResponse<object>?> UpdateQuantityAsync(
        CartUpdateViewModel request,
        CancellationToken cancellationToken = default)
    {
        return await PutAsync<CartUpdateViewModel, object>(
            ApiEndpoints.Cart.Update,
            request,
            cancellationToken);
    }

    public async Task<ApiResponse<object>?> RemoveFromCartAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        return await DeleteAsync(
            $"{ApiEndpoints.Cart.Remove}/{productId}",
            cancellationToken);
    }


}