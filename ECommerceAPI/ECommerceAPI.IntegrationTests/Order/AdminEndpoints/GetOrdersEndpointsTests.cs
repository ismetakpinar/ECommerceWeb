using ECommerceAPI.Dto.Dtos.CartDtos;
using ECommerceAPI.Dto.Dtos.OrderDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Order;

public class GetOrdersEndpointsTests
    : IntegrationTestBase
{
    public GetOrdersEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetAll_Should_Return_Ok_When_Admin()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var addressId = await CreateAddressAsync();

        var productId = await CreateProductAsync();

        await AddProductToCartAsync(
            productId,
            quantity: 2);

        var checkoutDto = new CheckoutDto
        {
            AddressId = addressId
        };

        var checkoutResponse = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/checkout",
            checkoutDto);

        checkoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<List<OrderListDto>>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Siparişler başarıyla listelendi.");

        result.Data.Should().NotBeNull();
        result.Data.Should().Contain(x => x.OrderId > 0);
    }

    [Fact]
    public async Task GetAll_Should_Return_Empty_When_There_Are_No_Orders()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<List<OrderListDto>>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Siparişler başarıyla listelendi.");

        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}