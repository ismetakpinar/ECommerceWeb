namespace ECommerce.Web.ViewModels.Product;

public sealed class UpdateProductViewModel
{
    public int ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public decimal Price { get; init; }

    public int Stock { get; init; }

    public int CategoryId { get; init; }
}