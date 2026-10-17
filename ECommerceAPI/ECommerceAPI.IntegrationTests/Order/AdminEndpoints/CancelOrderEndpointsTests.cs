using ECommerceAPI.Dto.Dtos.CartDtos;
using ECommerceAPI.Dto.Dtos.OrderDtos;
using ECommerceAPI.Dto.Dtos.ProductDtos;
using ECommerceAPI.Entity.Enums;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Order;

public class CancelOrderEndpointsTests
    : IntegrationTestBase
{
    public CancelOrderEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Cancel_Should_Return_Ok_When_Order_Is_Pending()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var addressId = await CreateAddressAsync();

        var productId = await CreateProductAsync();

        await AddProductToCartAsync(
            productId,
            quantity: 2);

        var checkoutResponse = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/checkout",
            new CheckoutDto
            {
                AddressId = addressId
            });

        checkoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var ordersResponse =
            await Client.GetAsync($"{ApiVersion}/Orders");

        var orders =
            await ReadResponseAsync<List<OrderListDto>>(ordersResponse);

        var orderId = orders.Data!
            .OrderByDescending(x => x.OrderId)
            .First()
            .OrderId;

        // Act
        var response =
            await Client.PutAsync(
                $"{ApiVersion}/Orders/{orderId}/cancel",
                null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Sipariş iptal edildi.");

        var detailResponse =
            await Client.GetAsync(
                $"{ApiVersion}/Orders/{orderId}");

        var detail =
            await ReadResponseAsync<OrderDetailDto>(detailResponse);

        detail.Data.Should().NotBeNull();
        detail.Data!.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task Cancel_Should_Return_BadRequest_When_Order_Is_Already_Delivered()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var addressId = await CreateAddressAsync();

        var productId = await CreateProductAsync();

        await AddProductToCartAsync(
            productId,
            quantity: 1);

        await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/checkout",
            new CheckoutDto
            {
                AddressId = addressId
            });

        var ordersResponse =
            await Client.GetAsync($"{ApiVersion}/Orders");

        var orders =
            await ReadResponseAsync<List<OrderListDto>>(ordersResponse);

        var orderId = orders.Data!
            .OrderByDescending(x => x.OrderId)
            .First()
            .OrderId;

        await Client.PutAsJsonAsync(
            $"{ApiVersion}/Orders",
            new UpdateOrderDto
            {
                OrderId = orderId,
                Status = OrderStatus.Preparing
            });

        await Client.PutAsJsonAsync(
            $"{ApiVersion}/Orders",
            new UpdateOrderDto
            {
                OrderId = orderId,
                Status = OrderStatus.Shipped
            });

        await Client.PutAsJsonAsync(
            $"{ApiVersion}/Orders",
            new UpdateOrderDto
            {
                OrderId = orderId,
                Status = OrderStatus.Delivered
            });

        // Act
        var response =
            await Client.PutAsync(
                $"{ApiVersion}/Orders/{orderId}/cancel",
                null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cancel_Should_Return_NotFound_When_Order_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response =
            await Client.PutAsync(
                $"{ApiVersion}/Orders/999999/cancel",
                null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Cancel_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response =
            await Client.PutAsync(
                $"{ApiVersion}/Orders/1/cancel",
                null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Cancel_Should_Restore_Product_Stock()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var addressId = await CreateAddressAsync();

        var productId = await CreateProductAsync();

        await AddProductToCartAsync(
            productId,
            quantity: 3);

        await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/checkout",
            new CheckoutDto
            {
                AddressId = addressId
            });

        var productBeforeResponse =
            await Client.GetAsync(
                $"{ApiVersion}/Products/{productId}");

        var productBefore =
            await ReadResponseAsync<ProductDetailDto>(
                productBeforeResponse);

        var stockAfterCheckout =
            productBefore.Data!.Stock;

        var ordersResponse =
            await Client.GetAsync(
                $"{ApiVersion}/Orders");

        var orders =
            await ReadResponseAsync<List<OrderListDto>>(
                ordersResponse);

        var orderId = orders.Data!
            .OrderByDescending(x => x.OrderId)
            .First()
            .OrderId;

        // Act
        var cancelResponse =
            await Client.PutAsync(
                $"{ApiVersion}/Orders/{orderId}/cancel",
                null);

        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert
        var productAfterResponse =
            await Client.GetAsync(
                $"{ApiVersion}/Products/{productId}");

        var productAfter =
            await ReadResponseAsync<ProductDetailDto>(
                productAfterResponse);

        productAfter.Data!.Stock
            .Should()
            .Be(stockAfterCheckout + 3);
    }
}