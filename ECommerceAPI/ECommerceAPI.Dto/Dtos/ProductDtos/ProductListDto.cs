namespace ECommerceAPI.Dto.Dtos.ProductDtos;

public class ProductListDto
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsFeatured { get; set; }

}