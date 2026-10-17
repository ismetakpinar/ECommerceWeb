using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;

namespace ECommerceAPI.IntegrationTests.Categories;

public class CategoryDeleteEndpointsTests : IntegrationTestBase
{
    public CategoryDeleteEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }



    [Fact]
    public async Task Delete_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response = await Client.DeleteAsync(
            $"{ApiVersion}/Categories/{TestConstants.DefaultCategoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_Should_Return_NotFound_When_Category_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.DeleteAsync(
            $"{ApiVersion}/Categories/{TestConstants.InvalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Kategori bulunamadı.");
    }

    [Fact]
    public async Task Delete_Should_Return_BadRequest_When_Category_Has_Products()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Seed datada ürün bulunan kategori
        const int categoryId = TestConstants.DefaultCategoryId;

        // Act
        var response = await Client.DeleteAsync(
            $"{ApiVersion}/Categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should()
            .Be("İçerisinde ürün bulunan kategori silinemez.");
    }
}