using ECommerceAPI.Dto.Dtos.CategoryDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Categories;

public class CategoryCreateEndpointsTests : IntegrationTestBase
{
    public CategoryCreateEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Create_Should_Return_Created_When_Request_Is_Valid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = new CreateCategoryDto
        {
            Name = $"Test Category {Guid.NewGuid()}"
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Categories",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result =
            await ReadResponseAsync<string>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Kategori başarıyla oluşturuldu.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Create_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        var dto = new CreateCategoryDto
        {
            Name = $"Test Category {Guid.NewGuid()}"
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Categories",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_Should_Return_BadRequest_When_Category_Name_Already_Exists()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = new CreateCategoryDto
        {
            Name = "Elektronik"
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Categories",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should()
            .Be("Bu isimde bir kategori zaten bulunmaktadır.");
    }

    [Fact]
    public async Task Create_Should_Return_BadRequest_When_Model_Is_Invalid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = new CreateCategoryDto
        {
            Name = string.Empty
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Categories",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().NotBeEmpty();
    }
}