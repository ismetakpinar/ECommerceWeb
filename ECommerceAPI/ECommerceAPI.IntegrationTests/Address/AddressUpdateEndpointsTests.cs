using ECommerceAPI.Dto.Dtos.AddressDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Address;

public class AddressUpdateEndpointsTests
    : IntegrationTestBase
{
    public AddressUpdateEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Update_Should_Return_Ok()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var addressId = await CreateAddressAsync();

        var dto = new UpdateAddressDto
        {
            AddressId = addressId,
            Title = "İş",
            City = "Ankara",
            District = "Çankaya",
            FullAddress = "Atatürk Bulvarı No:10"
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Addresses",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Adres başarıyla güncellendi.");
    }

    [Fact]
    public async Task Update_Should_Return_NotFound()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = new UpdateAddressDto
        {
            AddressId = 99999,
            Title = "İş",
            City = "Ankara",
            District = "Çankaya",
            FullAddress = "Atatürk Bulvarı No:10"
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Addresses",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Adres bulunamadı.");
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_Address_Already_Exists()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var firstAddressId = await CreateAddressAsync();

        await CreateAddressAsync(new CreateAddressDto
        {
            Title = "İş",
            City = "Ankara",
            District = "Çankaya",
            FullAddress = "Atatürk Bulvarı No:10"
        });

        var dto = new UpdateAddressDto
        {
            AddressId = firstAddressId,
            Title = "İş",
            City = "Ankara",
            District = "Çankaya",
            FullAddress = "Atatürk Bulvarı No:10"
        };

        // Act
        var response = await Client.PutAsJsonAsync(
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
    public async Task Update_Should_Return_BadRequest_When_Model_Is_Invalid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var addressId = await CreateAddressAsync();

        var dto = new UpdateAddressDto
        {
            AddressId = addressId,
            Title = "",
            City = "",
            District = "",
            FullAddress = ""
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Addresses",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Update_Should_Return_Unauthorized()
    {
        // Arrange
        ClearAuthentication();

        var dto = new UpdateAddressDto
        {
            AddressId = 1,
            Title = "İş",
            City = "Ankara",
            District = "Çankaya",
            FullAddress = "Atatürk Bulvarı No:10"
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Addresses",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}