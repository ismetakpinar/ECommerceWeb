using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;

namespace ECommerceAPI.IntegrationTests.Products;

public class ProductDeleteEndpointsTests
    : IntegrationTestBase
{
    public ProductDeleteEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Delete_Should_Return_Success_When_Product_Exists()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        // Act
        var response = await Client.DeleteAsync(
            $"{ApiVersion}/Products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        ClearAuthentication();

        // Act
        var response = await Client.DeleteAsync(
            $"{ApiVersion}/Products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.DeleteAsync(
            $"{ApiVersion}/Products/{TestConstants.InvalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Should_Not_Return_Deleted_Product()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync(
            $"{ApiVersion}/Products/{productId}");

        // Assert Delete
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var getResponse = await Client.GetAsync(
            $"{ApiVersion}/Products/{productId}");

        // Assert Get
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}