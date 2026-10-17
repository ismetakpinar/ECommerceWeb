using ECommerceAPI.Dto.Dtos.AddressDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;

namespace ECommerceAPI.IntegrationTests.Address;

public class AddressGetByIdEndpointsTests
    : IntegrationTestBase
{
    public AddressGetByIdEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetById_Should_Return_Ok()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var addressId = await CreateAddressAsync();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Addresses/{addressId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<AddressDetailDto>(response);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        var address = result.Data!;

        address.AddressId.Should().Be(addressId);
        address.Title.Should().Be("Ev");
        address.City.Should().Be("İstanbul");
        address.District.Should().Be("Kadıköy");
        address.FullAddress.Should().Be("Test Mahallesi No:1");
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        const int addressId = 99999;

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Addresses/{addressId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Adres bulunamadı.");
    }

    [Fact]
    public async Task GetById_Should_Return_Unauthorized()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Addresses/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}