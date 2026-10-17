namespace ECommerce.Web.ViewModels.Cart;

public sealed class CartItemViewModel
{
    public int CartItemId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal TotalAmount => UnitPrice * Quantity;
    public IReadOnlyCollection<CartItemViewModel> Items
    {
        get;
        set;
    } = [];

    public decimal SubTotal =>
        Items.Sum(x => x.TotalAmount);

    public decimal Shipping { get; set; }

    public decimal Total =>
        SubTotal + Shipping;
}
