using ECommerceAPI.Dto.Dtos.CartDtos;
using ECommerceAPI.Dto.Dtos.OrderDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Order;

public class GetMyOrderDetailEndpointsTests
    : IntegrationTestBase
{
    public GetMyOrderDetailEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetMyOrderDetail_Should_Return_Order_When_Order_Exists()
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

        var ordersResponse = await Client.GetAsync(
            $"{ApiVersion}/Orders/my-orders");

        ordersResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var orders =
            await ReadResponseAsync<List<OrderListDto>>(ordersResponse);

        orders.Data.Should().NotBeNull();

        var order = orders.Data!.Single();
        var orderId = order.OrderId;

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Orders/my-orders/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<OrderDetailDto>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Sipariş detayı başarıyla getirildi.");

        result.Data.Should().NotBeNull();

        var detail = result.Data!;

        detail.OrderId.Should().Be(orderId);
        detail.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetMyOrderDetail_Should_Return_NotFound_When_Order_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Orders/my-orders/{TestConstants.InvalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetMyOrderDetail_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Orders/my-orders/{TestConstants.InvalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}