using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Controllers;
using ECommerceAPI.Dto.Dtos.CartDtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ECommerceAPI.Tests.Controllers;

public class CartControllerTests
{
    private readonly Mock<ICartService> _cartServiceMock;
    private readonly CartController _controller;

    public CartControllerTests()
    {
        _cartServiceMock = new Mock<ICartService>();

        _controller = new CartController(
            _cartServiceMock.Object);
    }

    [Fact]
    public async Task AddToCart_ReturnsOk()
    {
        // Arrange
        var dto = new AddToCartDto
        {
            ProductId = 1,
            Quantity = 2
        };

        // Act
        var result = await _controller.AddToCart(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _cartServiceMock.Verify(
            x => x.AddToCartAsync(
                dto,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMyCart_ReturnsOk()
    {
        // Arrange
        var cart = new CartDto
        {
            CartId = 1,
            TotalItemCount = 2,
            TotalAmount = 350,
            Items = new()
        };

        _cartServiceMock
            .Setup(x => x.GetMyCartAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        // Act
        var result = await _controller.GetMyCart();

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _cartServiceMock.Verify(
            x => x.GetMyCartAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCartItem_ReturnsOk()
    {
        // Arrange
        var dto = new UpdateCartItemDto
        {
            ProductId = 1,
            Quantity = 5
        };

        // Act
        var result = await _controller.UpdateCartItem(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _cartServiceMock.Verify(
            x => x.UpdateCartItemAsync(
                dto,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task RemoveCartItem_ReturnsOk()
    {
        // Act
        var result = await _controller.RemoveCartItem(5);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _cartServiceMock.Verify(
            x => x.RemoveCartItemAsync(
                5,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Checkout_ReturnsOk()
    {
        // Arrange
        var dto = new CheckoutDto
        {
            AddressId = 3
        };

        // Act
        var result = await _controller.Checkout(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _cartServiceMock.Verify(
            x => x.CheckoutAsync(
                dto,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddToCart_ReturnsCorrectApiResponse()
    {
        var dto = new AddToCartDto
        {
            ProductId = 1,
            Quantity = 2
        };

        var result = await _controller.AddToCart(dto);

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<object>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Ürün sepete eklendi.");
    }

    [Fact]
    public async Task GetMyCart_ReturnsCorrectApiResponse()
    {
        var cart = new CartDto
        {
            CartId = 1,
            TotalItemCount = 2,
            TotalAmount = 350,
            Items = new()
        };

        _cartServiceMock
            .Setup(x => x.GetMyCartAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var result = await _controller.GetMyCart();

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<CartDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data!.CartId.Should().Be(1);
        response.Message.Should().Be("Sepet başarıyla getirildi.");
    }

    [Fact]
    public async Task UpdateCartItem_ReturnsCorrectApiResponse()
    {
        var dto = new UpdateCartItemDto
        {
            ProductId = 1,
            Quantity = 5
        };

        var result = await _controller.UpdateCartItem(dto);

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<object>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Sepet güncellendi.");
    }

    [Fact]
    public async Task RemoveCartItem_ReturnsCorrectApiResponse()
    {
        var result = await _controller.RemoveCartItem(5);

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<object>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Ürün sepetten kaldırıldı.");
    }

    [Fact]
    public async Task Checkout_ReturnsCorrectApiResponse()
    {
        var dto = new CheckoutDto
        {
            AddressId = 3
        };

        var result = await _controller.Checkout(dto);

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<object>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Sipariş başarıyla oluşturuldu.");
    }
}