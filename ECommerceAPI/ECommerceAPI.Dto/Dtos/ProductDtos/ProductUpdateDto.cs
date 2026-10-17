namespace ECommerceAPI.Dto.Dtos.ProductDtos;

public class ProductUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public bool IsFeatured { get; set; }
}