namespace ECommerce.Web.ViewModels.Product;

public sealed class ProductDetailViewModel
{
    public int ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public int Stock { get; init; }

    public string Description { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string ImageUrl { get; init; } = string.Empty;

    public bool IsFeatured { get; init; }
}