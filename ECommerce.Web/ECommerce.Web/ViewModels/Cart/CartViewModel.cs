namespace ECommerce.Web.ViewModels.Cart;

public sealed class CartViewModel
{
    public IReadOnlyCollection<CartItemViewModel> Items { get; set; } = [];

    public decimal SubTotal =>
        Items.Sum(x => x.TotalAmount);

    public decimal Shipping { get; set; }

    public decimal Total =>
        SubTotal + Shipping;
}