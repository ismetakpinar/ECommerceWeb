namespace ECommerceAPI.Dto.Dtos.CategoryDtos;

public class CategoryDetailDto
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int ProductCount { get; set; }
}
