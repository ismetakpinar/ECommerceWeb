using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;

namespace ECommerceAPI.IntegrationTests.Address;

public class AddressDeleteEndpointsTests
    : IntegrationTestBase
{
    public AddressDeleteEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Delete_Should_Return_Ok()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var addressId = await CreateAddressAsync();

        // Act
        var response = await Client.DeleteAsync(
            $"{ApiVersion}/Addresses/{addressId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Adres başarıyla silindi.");
    }

    [Fact]
    public async Task Delete_Should_Return_NotFound()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        const int addressId = 99999;

        // Act
        var response = await Client.DeleteAsync(
            $"{ApiVersion}/Addresses/{addressId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Adres bulunamadı.");
    }

    [Fact]
    public async Task Delete_Should_Return_Unauthorized()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response = await Client.DeleteAsync(
            $"{ApiVersion}/Addresses/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}