using ECommerceAPI.Dto.Dtos.CartDtos;

namespace ECommerceAPI.Business.Abstract;

public interface ICartService
{
    Task AddToCartAsync(
        AddToCartDto dto,
        CancellationToken cancellationToken = default);

    Task<CartDto> GetMyCartAsync(
        CancellationToken cancellationToken = default);

    Task UpdateCartItemAsync(
        UpdateCartItemDto dto,
        CancellationToken cancellationToken = default);

    Task CheckoutAsync(
        CheckoutDto dto,
        CancellationToken cancellationToken = default);

    Task RemoveCartItemAsync(
        int productId,
        CancellationToken cancellationToken = default);

}