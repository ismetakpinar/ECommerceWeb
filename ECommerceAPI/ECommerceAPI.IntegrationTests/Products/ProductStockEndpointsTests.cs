using ECommerceAPI.Dto.Dtos.ProductDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Products;

public class ProductStockEndpointsTests
    : IntegrationTestBase
{
    public ProductStockEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task IncreaseStock_Should_Return_Success_When_Request_Is_Valid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        var dto = new StockUpdateDto
        {
            Quantity = 10
        };

        // Act
        var response = await Client.PatchAsJsonAsync(
            $"{ApiVersion}/Products/{productId}/stock/increase",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task IncreaseStock_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        ClearAuthentication();

        var dto = new StockUpdateDto
        {
            Quantity = 5
        };

        // Act
        var response = await Client.PatchAsJsonAsync(
            $"{ApiVersion}/Products/{productId}/stock/increase",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task IncreaseStock_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = new StockUpdateDto
        {
            Quantity = 5
        };

        // Act
        var response = await Client.PatchAsJsonAsync(
            $"{ApiVersion}/Products/{TestConstants.InvalidId}/stock/increase",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DecreaseStock_Should_Return_Success_When_Request_Is_Valid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        var dto = new StockUpdateDto
        {
            Quantity = 5
        };

        // Act
        var response = await Client.PatchAsJsonAsync(
            $"{ApiVersion}/Products/{productId}/stock/decrease",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DecreaseStock_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        ClearAuthentication();

        var dto = new StockUpdateDto
        {
            Quantity = 5
        };

        // Act
        var response = await Client.PatchAsJsonAsync(
            $"{ApiVersion}/Products/{productId}/stock/decrease",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DecreaseStock_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = new StockUpdateDto
        {
            Quantity = 5
        };

        // Act
        var response = await Client.PatchAsJsonAsync(
            $"{ApiVersion}/Products/{TestConstants.InvalidId}/stock/decrease",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DecreaseStock_Should_Return_BadRequest_When_Quantity_Exceeds_Stock()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        var dto = new StockUpdateDto
        {
            Quantity = 99999
        };

        // Act
        var response = await Client.PatchAsJsonAsync(
            $"{ApiVersion}/Products/{productId}/stock/decrease",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}