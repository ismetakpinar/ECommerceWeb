using ECommerceAPI.Dto.Dtos.AddressDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;

namespace ECommerceAPI.IntegrationTests.Address;

public class AddressGetAllEndpointsTests
    : IntegrationTestBase
{
    public AddressGetAllEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetAll_Should_Return_Ok()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        await CreateAddressAsync();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Addresses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<List<AddressListDto>>(response);

        result.Success.Should().BeTrue();

        result.Data.Should().NotBeNull();

        var addresses = result.Data!;

        addresses.Should().HaveCount(1);

        var address = addresses.Single();

        address.Title.Should().Be("Ev");
        address.City.Should().Be("İstanbul");
        address.District.Should().Be("Kadıköy");
    }

    [Fact]
    public async Task GetAll_Should_Return_Unauthorized()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Addresses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}