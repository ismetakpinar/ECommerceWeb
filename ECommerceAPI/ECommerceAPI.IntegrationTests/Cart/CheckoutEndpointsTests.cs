using ECommerceAPI.Dto.Dtos.CartDtos;
using ECommerceAPI.Dto.Dtos.ProductDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Cart;

public class CheckoutEndpointsTests
    : IntegrationTestBase
{
    public CheckoutEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Checkout_Should_Return_Ok_When_Request_Is_Valid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var addressId = await CreateAddressAsync();
        var productId = await CreateProductAsync();

        await AddProductToCartAsync(productId, quantity: 2);

        var dto = new CheckoutDto
        {
            AddressId = addressId
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/checkout",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await ReadResponseAsync<object>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Sipariş başarıyla oluşturuldu.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Checkout_Should_Clear_Cart_When_Successful()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var addressId = await CreateAddressAsync();
        var productId = await CreateProductAsync();

        await AddProductToCartAsync(productId, quantity: 2);

        var checkoutDto = new CheckoutDto
        {
            AddressId = addressId
        };

        // Act
        var checkoutResponse = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/checkout",
            checkoutDto);

        // Assert Checkout
        checkoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var cartResponse = await Client.GetAsync(
            $"{ApiVersion}/Cart");

        // Assert Cart
        cartResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cart =
            await ReadResponseAsync<CartDto>(cartResponse);

        cart.Success.Should().BeTrue();

        cart.Data.Should().NotBeNull();

        var cartData = cart.Data!;

        cartData.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Checkout_Should_Update_Product_Stock_When_Successful()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var addressId = await CreateAddressAsync();
        var productId = await CreateProductAsync();

        var productResponseBefore = await Client.GetAsync(
            $"{ApiVersion}/Products/{productId}");

        productResponseBefore.StatusCode.Should().Be(HttpStatusCode.OK);

        var productBefore =
            await ReadResponseAsync<ProductDetailDto>(productResponseBefore);

        productBefore.Data.Should().NotBeNull();

        var before = productBefore.Data!;

        var initialStock = before.Stock;

        const int quantity = 2;

        await AddProductToCartAsync(productId, quantity);

        var checkoutDto = new CheckoutDto
        {
            AddressId = addressId
        };

        // Act
        var checkoutResponse = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/checkout",
            checkoutDto);

        // Assert Checkout
        checkoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var productResponseAfter = await Client.GetAsync(
            $"{ApiVersion}/Products/{productId}");

        // Assert Product
        productResponseAfter.StatusCode.Should().Be(HttpStatusCode.OK);

        var productAfter =
            await ReadResponseAsync<ProductDetailDto>(productResponseAfter);

        productAfter.Data.Should().NotBeNull();

        var after = productAfter.Data!;

        after.Stock.Should().Be(initialStock - quantity);
    }

    [Fact]
    public async Task Checkout_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var addressId = await CreateAddressAsync();
        var productId = await CreateProductAsync();

        await AddProductToCartAsync(productId, quantity: 2);

        ClearAuthentication();

        var checkoutDto = new CheckoutDto
        {
            AddressId = addressId
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/checkout",
            checkoutDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Checkout_Should_Return_BadRequest_When_Cart_Is_Empty()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var addressId = await CreateAddressAsync();

        var checkoutDto = new CheckoutDto
        {
            AddressId = addressId
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/checkout",
            checkoutDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Sepet boş.");
    }

    [Fact]
    public async Task Checkout_Should_Return_NotFound_When_Address_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        await AddProductToCartAsync(productId, quantity: 2);

        var checkoutDto = new CheckoutDto
        {
            AddressId = TestConstants.InvalidId
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/checkout",
            checkoutDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Adres bulunamadı veya bu kullanıcıya ait değil.");
    }
}