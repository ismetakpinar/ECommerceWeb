namespace ECommerce.Web.ViewModels.Cart;

public sealed class CartAddViewModel
{
    public int ProductId { get; init; }

    public int Quantity { get; init; } = 1;
}