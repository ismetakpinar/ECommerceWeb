using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Dto.Dtos.ProductDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;

namespace ECommerceAPI.IntegrationTests.Products;

public class ProductGetEndpointsTests : IntegrationTestBase
{
    public ProductGetEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetAll_Should_Return_Success_Response()
    {
        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<PagedResult<ProductListDto>>(response);

        result.Success.Should().BeTrue();

        result.Data.Should().NotBeNull();

        var page = result.Data!;

        page.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAll_Should_Respect_Page_And_PageSize()
    {
        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Products?page=1&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<PagedResult<ProductListDto>>(response);

        result.Success.Should().BeTrue();

        result.Data.Should().NotBeNull();

        var page = result.Data!;

        page.Page.Should().Be(1);
        page.PageSize.Should().Be(2);
        page.Items.Count.Should().BeLessOrEqualTo(2);
    }

    [Fact]
    public async Task GetById_Should_Return_Product_When_Product_Exists()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<ProductDetailDto>(response);

        result.Success.Should().BeTrue();

        result.Data.Should().NotBeNull();

        var product = result.Data!;

        product.ProductId.Should().Be(productId);
    }

    [Fact]
    public async Task GetById_Should_Return_Product_Details()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<ProductDetailDto>(response);

        result.Data.Should().NotBeNull();

        var product = result.Data!;

        product.Name.Should().NotBeNullOrWhiteSpace();
        product.Description.Should().NotBeNullOrWhiteSpace();
        product.Price.Should().BeGreaterThan(0);
        product.Stock.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetById_Should_Return_Category_Information()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<ProductDetailDto>(response);

        result.Data.Should().NotBeNull();

        var product = result.Data!;

        product.CategoryId.Should().BeGreaterThan(0);
        product.CategoryName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        var invalidId = TestConstants.InvalidId;

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Products/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}