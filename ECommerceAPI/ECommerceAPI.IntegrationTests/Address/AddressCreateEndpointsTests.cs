using ECommerceAPI.Dto.Dtos.AddressDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Address;

public class AddressCreateEndpointsTests
    : IntegrationTestBase
{
    public AddressCreateEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Create_Should_Return_Created()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = new CreateAddressDto
        {
            Title = "Ev",
            City = "İstanbul",
            District = "Kadıköy",
            FullAddress = "Test Mahallesi No:1"
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Addresses",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Adres başarıyla oluşturuldu.");
    }

    [Fact]
    public async Task Create_Should_Return_BadRequest_When_Address_Already_Exists()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = new CreateAddressDto
        {
            Title = "Ev",
            City = "İstanbul",
            District = "Kadıköy",
            FullAddress = "Test Mahallesi No:1"
        };

        await Client.PostAsJsonAsync(
            $"{ApiVersion}/Addresses",
            dto);

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Addresses",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Bu adres zaten kayıtlı.");
    }

    [Fact]
    public async Task Create_Should_Return_BadRequest_When_Model_Is_Invalid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = new CreateAddressDto
        {
            Title = "",
            City = "",
            District = "",
            FullAddress = ""
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Addresses",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Create_Should_Return_Unauthorized()
    {
        // Arrange
        ClearAuthentication();

        var dto = new CreateAddressDto
        {
            Title = "Ev",
            City = "İstanbul",
            District = "Kadıköy",
            FullAddress = "Test Mahallesi No:1"
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Addresses",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}