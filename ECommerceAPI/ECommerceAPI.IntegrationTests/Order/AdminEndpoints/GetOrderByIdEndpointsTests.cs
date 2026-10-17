using ECommerceAPI.Dto.Dtos.CartDtos;
using ECommerceAPI.Dto.Dtos.OrderDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Order;

public class GetOrderByIdEndpointsTests
    : IntegrationTestBase
{
    public GetOrderByIdEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetById_Should_Return_Order_When_Order_Exists()
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

        // Act
        var response =
            await Client.GetAsync(
                $"{ApiVersion}/Orders/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<OrderDetailDto>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Sipariş başarıyla getirildi.");

        result.Data.Should().NotBeNull();
        result.Data!.OrderId.Should().Be(orderId);
        result.Data.CustomerName.Should().NotBeNullOrWhiteSpace();
        result.Data.Status.Should().Be(
            ECommerceAPI.Entity.Enums.OrderStatus.Pending);
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Order_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response =
            await Client.GetAsync(
                $"{ApiVersion}/Orders/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response =
            await Client.GetAsync(
                $"{ApiVersion}/Orders/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}