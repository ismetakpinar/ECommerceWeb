using ECommerceAPI.Dto.Dtos.CategoryDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Categories;

public class CategoryUpdateEndpointsTests : IntegrationTestBase
{
    public CategoryUpdateEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Update_Should_Return_Ok_When_Request_Is_Valid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = new UpdateCategoryDto
        {
            CategoryId = TestConstants.DefaultCategoryId,
            Name = $"Updated Category {Guid.NewGuid()}"
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Categories",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<string>(response);

        result.Success.Should().BeTrue();
        result.Message.Should()
            .Be("Kategori başarıyla güncellendi.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Update_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        var dto = new UpdateCategoryDto
        {
            CategoryId = TestConstants.DefaultCategoryId,
            Name = "Updated Category"
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Categories",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_Should_Return_NotFound_When_Category_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = new UpdateCategoryDto
        {
            CategoryId = TestConstants.InvalidId,
            Name = "Updated Category"
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Categories",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Kategori bulunamadı.");
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_Category_Name_Already_Exists()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = new UpdateCategoryDto
        {
            CategoryId = TestConstants.DefaultCategoryId,
            Name = "Giyim"
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Categories",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should()
            .Be("Bu isimde başka bir kategori bulunmaktadır.");
    }
}