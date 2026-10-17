using ECommerceAPI.Dto.Dtos.CartDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Cart;

public class CartEndpointsTests
    : IntegrationTestBase
{
    public CartEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task AddToCart_Should_Return_Ok_When_Request_Is_Valid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        var request = new AddToCartDto
        {
            ProductId = productId,
            Quantity = 2
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/add",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Ürün sepete eklendi.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task AddToCart_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        var request = new AddToCartDto
        {
            ProductId = 1,
            Quantity = 1
        };

        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/add",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddToCart_Should_Return_BadRequest_When_Model_Is_Invalid()
    {
        await AuthenticateAsAdminAsync();

        var request = new AddToCartDto
        {
            ProductId = 0,
            Quantity = 0
        };

        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/add",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
    }

    [Fact]
    public async Task AddToCart_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        await AuthenticateAsAdminAsync();

        var request = new AddToCartDto
        {
            ProductId = int.MaxValue,
            Quantity = 1
        };

        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/add",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Ürün bulunamadı.");
    }

    [Fact]
    public async Task AddToCart_Should_Return_BadRequest_When_Quantity_Exceeds_Stock()
    {
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        var request = new AddToCartDto
        {
            ProductId = productId,
            Quantity = 99999
        };

        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/add",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("İstenen ürün miktarı stok miktarını aşamaz.");
    }

    [Fact]
    public async Task AddToCart_Should_Return_BadRequest_When_Total_Quantity_Exceeds_Stock()
    {
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync(
            TestDataBuilder.CreateProduct(stock: 100));

        var firstRequest = new AddToCartDto
        {
            ProductId = productId,
            Quantity = 90
        };

        var firstResponse = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/add",
            firstRequest);

        firstResponse.EnsureSuccessStatusCode();

        var secondRequest = new AddToCartDto
        {
            ProductId = productId,
            Quantity = 20
        };

        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/add",
            secondRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Sepetteki toplam ürün miktarı stok miktarını aşamaz.");
    }

    [Fact]
    public async Task GetMyCart_Should_Return_Ok_When_Cart_Has_Items()
    {
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync(
            TestDataBuilder.CreateProduct(
                price: 100,
                stock: 20));

        await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/add",
            new AddToCartDto
            {
                ProductId = productId,
                Quantity = 2
            });

        var response = await Client.GetAsync(
            $"{ApiVersion}/Cart");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<CartDto>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Sepet başarıyla getirildi.");

        result.Data.Should().NotBeNull();

        var cart = result.Data!;

        cart.Items.Should().HaveCount(1);
        cart.TotalItemCount.Should().Be(2);
        cart.TotalAmount.Should().Be(200);

        var item = cart.Items.Single();

        item.ProductId.Should().Be(productId);
        item.Quantity.Should().Be(2);
        item.UnitPrice.Should().Be(100);
        item.LineTotal.Should().Be(200);
    }

    [Fact]
    public async Task GetMyCart_Should_Return_Empty_Cart_When_User_Has_No_Cart()
    {
        await AuthenticateAsAdminAsync();

        var response = await Client.GetAsync(
            $"{ApiVersion}/Cart");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<CartDto>(response);

        result.Success.Should().BeTrue();

        result.Data.Should().NotBeNull();

        var cart = result.Data!;

        cart.Items.Should().BeEmpty();
        cart.TotalItemCount.Should().Be(0);
        cart.TotalAmount.Should().Be(0);
    }
    [Fact]
    public async Task GetMyCart_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Cart");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCartItem_Should_Return_Ok_When_Request_Is_Valid()
    {
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync(
            TestDataBuilder.CreateProduct(stock: 20));

        await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/add",
            new AddToCartDto
            {
                ProductId = productId,
                Quantity = 2
            });

        var request = new UpdateCartItemDto
        {
            ProductId = productId,
            Quantity = 5
        };

        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Cart/update",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Sepet güncellendi.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCartItem_Should_Remove_Item_When_Quantity_Is_Zero()
    {
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/add",
            new AddToCartDto
            {
                ProductId = productId,
                Quantity = 2
            });

        var request = new UpdateCartItemDto
        {
            ProductId = productId,
            Quantity = 0
        };

        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Cart/update",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cartResponse =
            await Client.GetAsync($"{ApiVersion}/Cart");

        var cart =
            await ReadResponseAsync<CartDto>(cartResponse);

        cart.Data.Should().NotBeNull();

        var cartData = cart.Data!;

        cartData.Items
            .Any(x => x.ProductId == productId)
            .Should().BeFalse();
    }

    [Fact]
    public async Task UpdateCartItem_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        var request = new UpdateCartItemDto
        {
            ProductId = 1,
            Quantity = 5
        };

        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Cart/update",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCartItem_Should_Return_BadRequest_When_Model_Is_Invalid()
    {
        await AuthenticateAsAdminAsync();

        var request = new UpdateCartItemDto
        {
            ProductId = 0,
            Quantity = -1
        };

        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Cart/update",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateCartItem_Should_Return_NotFound_When_Cart_Item_Does_Not_Exist()
    {
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/add",
            new AddToCartDto
            {
                ProductId = productId,
                Quantity = 1
            });

        var request = new UpdateCartItemDto
        {
            ProductId = int.MaxValue,
            Quantity = 2
        };

        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Cart/update",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Ürün sepette bulunamadı.");
    }

    [Fact]
    public async Task UpdateCartItem_Should_Return_BadRequest_When_Quantity_Exceeds_Stock()
    {
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync(
            TestDataBuilder.CreateProduct(stock: 10));

        await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/add",
            new AddToCartDto
            {
                ProductId = productId,
                Quantity = 2
            });

        var request = new UpdateCartItemDto
        {
            ProductId = productId,
            Quantity = 20
        };

        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Cart/update",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("İstenen ürün miktarı stok miktarını aşamaz.");
    }
    [Fact]
    public async Task RemoveCartItem_Should_Return_Ok_When_Product_Exists_In_Cart()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productId = await CreateProductAsync();

        await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/add",
            new AddToCartDto
            {
                ProductId = productId,
                Quantity = 2
            });

        // Act
        var response = await Client.DeleteAsync(
            $"{ApiVersion}/Cart/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<string>(response);

        result.Success.Should().BeTrue();

        var cartResponse =
            await Client.GetAsync($"{ApiVersion}/Cart");

        var cart =
            await ReadResponseAsync<CartDto>(cartResponse);

        cart.Data.Should().NotBeNull();

        var cartData = cart.Data!;

        cartData.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveCartItem_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Act
        var response =
            await Client.DeleteAsync($"{ApiVersion}/Cart/1");

        // Assert
        response.StatusCode.Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveCartItem_Should_Return_NotFound_When_Cart_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response =
            await Client.DeleteAsync($"{ApiVersion}/Cart/999");

        // Assert
        response.StatusCode.Should()
            .Be(HttpStatusCode.NotFound);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();

        result.Message.Should()
            .Be("Sepet bulunamadı.");
    }

    [Fact]
    public async Task RemoveCartItem_Should_Return_NotFound_When_Product_Is_Not_In_Cart()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var productInCartId = await CreateProductAsync();

        await AddProductToCartAsync(productInCartId);

        var productNotInCartId = await CreateProductAsync(
            TestDataBuilder.CreateProduct(
                name: "Cartta Olmayan Ürün"));

        // Act
        var response = await Client.DeleteAsync(
            $"{ApiVersion}/Cart/{productNotInCartId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Ürün sepette bulunamadı.");
    }
}