namespace ECommerceAPI.Dto.Dtos.ProductDtos;

public class ProductDetailDto
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public bool IsFeatured { get; set; }
}