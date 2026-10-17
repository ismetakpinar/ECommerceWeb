using ECommerceAPI.Dto.Dtos.ProductDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Products;

public class ProductCreateEndpointsTests : IntegrationTestBase
{
    public ProductCreateEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Create_Should_Return_Success_When_Product_Is_Valid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = TestDataBuilder.CreateProduct();

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Products",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        response.Headers.Location.Should().NotBeNull();

        var result =
            await ReadResponseAsync<ProductDetailDto>(response);

        result.Success.Should().BeTrue();

        result.Data.Should().NotBeNull();

        var product = result.Data!;

        product.Name.Should().Be(dto.Name);
        product.Description.Should().Be(dto.Description);
        product.Price.Should().Be(dto.Price);
        product.Stock.Should().Be(dto.Stock);
        product.CategoryId.Should().Be(dto.CategoryId);
    }

    [Fact]
    public async Task Create_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        var dto = TestDataBuilder.CreateProduct();

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Products",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_Should_Return_BadRequest_When_Model_Is_Invalid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = TestDataBuilder.CreateProduct(
            name: string.Empty);

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Products",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_Should_Return_NotFound_When_Category_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = TestDataBuilder.CreateProduct(
            categoryId: TestConstants.InvalidId);

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Products",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}