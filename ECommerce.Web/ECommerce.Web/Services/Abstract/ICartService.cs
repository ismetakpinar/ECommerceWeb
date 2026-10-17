using ECommerce.Web.Models;
using ECommerce.Web.ViewModels.Cart;

namespace ECommerce.Web.Services.Cart;

public interface ICartService
{
    Task<ApiResponse<object>?> AddToCartAsync(
        CartAddViewModel request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CartViewModel>?> GetCartAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object>?> RemoveFromCartAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object>?> UpdateQuantityAsync(
        CartUpdateViewModel request,
        CancellationToken cancellationToken = default);
}