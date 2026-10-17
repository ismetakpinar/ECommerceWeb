using ECommerceAPI.Dto.Dtos.CartDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Customer;

public class CustomerDeleteEndpointsTests
    : IntegrationTestBase
{
    public CustomerDeleteEndpointsTests(
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
            $"{ApiVersion}/Customers/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_Should_Return_NotFound_When_Customer_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        const int customerId = 99999;

        // Act
        var response = await Client.DeleteAsync(
            $"{ApiVersion}/Customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Müşteri bulunamadı.");
    }

    [Fact]
    public async Task Delete_Should_Return_BadRequest_When_Customer_Has_Orders()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var addressId = await CreateAddressAsync();

        var productId = await CreateProductAsync();

        await AddProductToCartAsync(
            productId,
            quantity: 1);

        var checkoutResponse = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/checkout",
            new CheckoutDto
            {
                AddressId = addressId
            });

        checkoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        const int customerId = 1;

        // Act
        var response = await Client.DeleteAsync(
            $"{ApiVersion}/Customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Siparişi bulunan müşteri silinemez.");
    }
}