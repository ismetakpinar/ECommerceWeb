using ECommerceAPI.Dto.Dtos.CategoryDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;

namespace ECommerceAPI.IntegrationTests.Categories;

public class CategoryGetEndpointsTests : IntegrationTestBase
{
    public CategoryGetEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetAllShouldReturnSuccessResponse()
    {
        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<List<CategoryListDto>>(response);

        result.Success.Should().BeTrue();

        result.Data.Should().NotBeNull();
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_Should_Return_Category_When_Category_Exists()
    {
        // Arrange
        const int categoryId = TestConstants.DefaultCategoryId;

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<CategoryDetailDto>(response);

        result.Success.Should().BeTrue();

        result.Data.Should().NotBeNull();

        var category = result.Data!;

        category.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task GetById_Should_Return_Category_Details()
    {
        // Arrange
        const int categoryId = TestConstants.DefaultCategoryId;

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<CategoryDetailDto>(response);

        result.Data.Should().NotBeNull();

        var category = result.Data!;

        category.Name.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Category_Does_Not_Exist()
    {
        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Categories/{TestConstants.InvalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}