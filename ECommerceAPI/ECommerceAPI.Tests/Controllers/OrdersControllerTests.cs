using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Controllers;
using ECommerceAPI.Dto.Dtos.OrderDtos;
using ECommerceAPI.Entity.Entities;
using ECommerceAPI.Entity.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ECommerceAPI.Tests.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _orderServiceMock = new Mock<IOrderService>();
        _mapperMock = new Mock<IMapper>();

        _controller = new OrdersController(
            _orderServiceMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        var orders = new List<Order>();

        var dtoList = new List<OrderListDto>
        {
            new()
            {
                OrderId = 1,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                CustomerName = "İsmet Akpınar"
            }
        };

        _orderServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        _mapperMock
            .Setup(x => x.Map<List<OrderListDto>>(orders))
            .Returns(dtoList);

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();

        _orderServiceMock.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<List<OrderListDto>>(orders),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        var order = new Order();

        var dto = new OrderDetailDto
        {
            OrderId = 5,
            CustomerId = 2,
            CustomerName = "Ali Veli",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            TotalAmount = 550,
            Items = new()
        };

        _orderServiceMock
            .Setup(x => x.GetByIdAsync(
                5,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mapperMock
            .Setup(x => x.Map<OrderDetailDto>(order))
            .Returns(dto);

        var result = await _controller.GetById(5);

        result.Should().BeOfType<OkObjectResult>();

        _orderServiceMock.Verify(
            x => x.GetByIdAsync(
                5,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<OrderDetailDto>(order),
            Times.Once);
    }

    [Fact]
    public async Task GetMyOrders_ReturnsOkResult()
    {
        var orders = new List<OrderListDto>
        {
            new()
            {
                OrderId = 2,
                CustomerName = "İsmet Akpınar",
                OrderDate = DateTime.Now,
                Status = OrderStatus.Delivered
            }
        };

        _orderServiceMock
            .Setup(x => x.GetMyOrdersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var result = await _controller.GetMyOrders();

        result.Should().BeOfType<OkObjectResult>();

        _orderServiceMock.Verify(
            x => x.GetMyOrdersAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task GetMyOrderDetail_ReturnsOkResult()
    {
        // Arrange
        var dto = new OrderDetailDto
        {
            OrderId = 3,
            CustomerId = 1,
            CustomerName = "İsmet Akpınar",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Delivered,
            TotalAmount = 1250,
            Items = new()
        };

        _orderServiceMock
            .Setup(x => x.GetMyOrderDetailAsync(
                3,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.GetMyOrderDetail(3);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _orderServiceMock.Verify(
            x => x.GetMyOrderDetailAsync(
                3,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_ReturnsOk()
    {
        // Arrange
        var dto = new UpdateOrderDto
        {
            OrderId = 1,
            Status = OrderStatus.Delivered
        };

        // Act
        var result = await _controller.Update(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _orderServiceMock.Verify(
            x => x.UpdateAsync(
                dto,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelOrder_ReturnsOk()
    {
        // Act
        var result = await _controller.CancelOrder(5);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _orderServiceMock.Verify(
            x => x.CancelOrderAsync(
                5,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_ReturnsCorrectApiResponse()
    {
        _orderServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        _mapperMock
            .Setup(x => x.Map<List<OrderListDto>>(It.IsAny<List<Order>>()))
            .Returns(new List<OrderListDto>());

        var result = await _controller.GetAll();

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<List<OrderListDto>>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Siparişler başarıyla listelendi.");
    }

    [Fact]
    public async Task GetById_ReturnsCorrectApiResponse()
    {
        var order = new Order();

        _orderServiceMock
            .Setup(x => x.GetByIdAsync(
                2,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mapperMock
            .Setup(x => x.Map<OrderDetailDto>(order))
            .Returns(new OrderDetailDto
            {
                OrderId = 2,
                CustomerId = 1,
                CustomerName = "Ali Veli",
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                TotalAmount = 250,
                Items = new()
            });

        var result = await _controller.GetById(2);

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<OrderDetailDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data!.OrderId.Should().Be(2);
    }

    [Fact]
    public async Task GetMyOrders_ReturnsCorrectApiResponse()
    {
        var orders = new List<OrderListDto>();

        _orderServiceMock
            .Setup(x => x.GetMyOrdersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var result = await _controller.GetMyOrders();

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<List<OrderListDto>>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Siparişler başarıyla getirildi.");
    }

    [Fact]
    public async Task GetMyOrderDetail_ReturnsCorrectApiResponse()
    {
        var dto = new OrderDetailDto
        {
            OrderId = 7,
            CustomerId = 1,
            CustomerName = "İsmet Akpınar",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Delivered,
            TotalAmount = 999,
            Items = new()
        };

        _orderServiceMock
            .Setup(x => x.GetMyOrderDetailAsync(
                7,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _controller.GetMyOrderDetail(7);

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<OrderDetailDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data!.OrderId.Should().Be(7);
        response.Message.Should().Be("Sipariş detayı başarıyla getirildi.");
    }

    [Fact]
    public async Task Update_ReturnsCorrectApiResponse()
    {
        var dto = new UpdateOrderDto
        {
            OrderId = 1,
            Status = OrderStatus.Delivered,
        };

        var result = await _controller.Update(dto);

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<object>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Sipariş durumu güncellendi.");
    }

    [Fact]
    public async Task CancelOrder_ReturnsCorrectApiResponse()
    {
        var result = await _controller.CancelOrder(5);

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<object>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Sipariş iptal edildi.");
    }
}