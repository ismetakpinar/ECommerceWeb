using ECommerceAPI.Dto.Dtos.ProductDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Products;

public class ProductUpdateEndpointsTests : IntegrationTestBase
{
    public ProductUpdateEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Update_Should_Return_Success_When_Product_Is_Valid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        var dto = TestDataBuilder.UpdateProduct();

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Products/{productId}",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<ProductDetailDto>(response);

        result.Success.Should().BeTrue();

        result.Data.Should().NotBeNull();

        var product = result.Data!;

        product.ProductId.Should().Be(productId);
        product.Name.Should().Be(dto.Name);
        product.Description.Should().Be(dto.Description);
        product.Price.Should().Be(dto.Price);
        product.CategoryId.Should().Be(dto.CategoryId);
    }

    [Fact]
    public async Task Update_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        ClearAuthentication();

        var dto = TestDataBuilder.UpdateProduct();

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Products/{productId}",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = TestDataBuilder.UpdateProduct();

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Products/{TestConstants.InvalidId}",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_Model_Is_Invalid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        var dto = TestDataBuilder.UpdateProduct(
            name: string.Empty);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Products/{productId}",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_Should_Return_NotFound_When_Category_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        var dto = TestDataBuilder.UpdateProduct(
            categoryId: TestConstants.InvalidId);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Products/{productId}",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}