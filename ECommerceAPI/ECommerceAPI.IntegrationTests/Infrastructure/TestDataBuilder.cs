using ECommerceAPI.Dto.Dtos.CategoryDtos;
using ECommerceAPI.Dto.Dtos.ProductDtos;

namespace ECommerceAPI.IntegrationTests.Infrastructure;

public static class TestDataBuilder
{
    public static CreateProductDto CreateProduct(
        string? name = null,
        string? description = null,
        decimal price = 100,
        int stock = 10,
        int categoryId = TestConstants.DefaultCategoryId)
    {
        return new CreateProductDto
        {
            Name = name ?? $"Product-{Guid.NewGuid()}",
            Description = description ?? "Integration Test Product",
            Price = price,
            Stock = stock,
            CategoryId = categoryId,
            ImageUrl = "/theme/img/product-1.jpg"
        };
    }

    public static ProductUpdateDto UpdateProduct(
        string? name = null,
        string? description = null,
        decimal price = 200,
        int categoryId = TestConstants.DefaultCategoryId)
    {
        return new ProductUpdateDto
        {
            Name = name ?? $"Updated-{Guid.NewGuid()}",
            Description = description ?? "Updated Product",
            Price = price,
            CategoryId = categoryId,
            ImageUrl = "/theme/img/product-1.jpg"
        };
    }

    public static CreateCategoryDto CreateCategory(
        string? name = null)
    {
        return new CreateCategoryDto
        {
            Name = name ?? $"Category-{Guid.NewGuid()}"
        };
    }

    public static UpdateCategoryDto UpdateCategory(
        string? name = null)
    {
        return new UpdateCategoryDto
        {
            Name = name ?? $"UpdatedCategory-{Guid.NewGuid()}"
        };
    }
}