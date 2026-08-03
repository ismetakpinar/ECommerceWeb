namespace ECommerce.Web.ViewModels.Product;

public sealed class ProductListViewModel
{
    public int ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public int Stock { get; init; }

    public string CategoryName { get; init; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsFeatured { get; set; }
}