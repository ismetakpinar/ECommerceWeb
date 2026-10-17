using ECommerceAPI.Dto.Dtos.CartDtos;
using ECommerceAPI.Dto.Dtos.OrderDtos;
using ECommerceAPI.Entity.Enums;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Order;

public class UpdateOrderEndpointsTests
    : IntegrationTestBase
{
    public UpdateOrderEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Update_Should_Return_Ok_When_Status_Is_Updated()
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

        var listResponse =
            await Client.GetAsync($"{ApiVersion}/Orders");

        var listResult =
            await ReadResponseAsync<List<OrderListDto>>(listResponse);

        var orderId = listResult.Data!
            .OrderByDescending(x => x.OrderId)
            .First()
            .OrderId;

        var dto = new UpdateOrderDto
        {
            OrderId = orderId,
            Status = OrderStatus.Preparing
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Orders",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Sipariş durumu güncellendi.");

        var getResponse =
            await Client.GetAsync($"{ApiVersion}/Orders/{orderId}");

        var order =
            await ReadResponseAsync<OrderDetailDto>(getResponse);

        order.Data!.Status.Should().Be(OrderStatus.Preparing);
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_Status_Transition_Is_Invalid()
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

        var listResponse =
            await Client.GetAsync($"{ApiVersion}/Orders");

        var listResult =
            await ReadResponseAsync<List<OrderListDto>>(listResponse);

        var orderId = listResult.Data!
            .OrderByDescending(x => x.OrderId)
            .First()
            .OrderId;

        var dto = new UpdateOrderDto
        {
            OrderId = orderId,
            Status = OrderStatus.Delivered
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Orders",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_Should_Return_NotFound_When_Order_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var dto = new UpdateOrderDto
        {
            OrderId = 999999,
            Status = OrderStatus.Preparing
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Orders",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        var dto = new UpdateOrderDto
        {
            OrderId = 1,
            Status = OrderStatus.Preparing
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Orders",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}