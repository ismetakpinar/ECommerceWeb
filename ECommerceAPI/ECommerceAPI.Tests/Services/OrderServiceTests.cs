using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Concrete;
using ECommerceAPI.DataAccess.Specifications;
using ECommerceAPI.DataAccess.UnitOfWorks;
using ECommerceAPI.Dto.Dtos.OrderDtos;
using ECommerceAPI.Entity.Entities;
using ECommerceAPI.Entity.Enums;
using ECommerceAPI.Entity.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ECommerceAPI.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICustomerService> _customerServiceMock;

    private readonly Mock<IGenericRepository<Order>>
        _orderRepositoryMock;

    private readonly Mock<IGenericRepository<Product>>
        _productRepositoryMock;

    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _unitOfWorkMock =
            new Mock<IUnitOfWork>();

        _mapperMock =
            new Mock<IMapper>();

        _currentUserServiceMock =
            new Mock<ICurrentUserService>();

        _customerServiceMock =
            new Mock<ICustomerService>();

        _orderRepositoryMock =
            new Mock<IGenericRepository<Order>>();

        _productRepositoryMock =
            new Mock<IGenericRepository<Product>>();

        _unitOfWorkMock
            .Setup(x => x.Repository<Order>())
            .Returns(_orderRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.Repository<Product>())
            .Returns(_productRepositoryMock.Object);

        _orderService = new OrderService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object,
            _customerServiceMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_OrdersExist_ReturnsOrderList()
    {
        // Arrange
        var orders = new List<Order>
        {
            new()
            {
                OrderId = 1,
                OrderDate = DateTime.UtcNow.AddDays(-2),
                Status = OrderStatus.Pending,
                CustomerId = 10
            },
            new()
            {
                OrderId = 2,
                OrderDate = DateTime.UtcNow.AddDays(-1),
                Status = OrderStatus.Preparing,
                CustomerId = 20
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync(
                It.IsAny<ISpecification<Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result =
            await _orderService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeSameAs(orders);

        _orderRepositoryMock.Verify(
            x => x.GetAllAsync(
                It.IsAny<OrderListSpecification>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_NoOrders_ReturnsEmptyList()
    {
        // Arrange
        _orderRepositoryMock
            .Setup(x => x.GetAllAsync(
                It.IsAny<ISpecification<Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        // Act
        var result =
            await _orderService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_OrderExists_ReturnsOrder()
    {
        // Arrange
        const int orderId = 15;

        var order = new Order
        {
            OrderId = orderId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            CustomerId = 5
        };

        _orderRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result =
            await _orderService.GetByIdAsync(orderId);

        // Assert
        result.Should().BeSameAs(order);
        result.OrderId.Should().Be(orderId);

        _orderRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<OrderByIdWithCustomerSpecification>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_OrderDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _orderRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        Func<Task> act = async () =>
            await _orderService.GetByIdAsync(999);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Sipariş bulunamadı.");
    }

    [Fact]
    public async Task GetMyOrdersAsync_OrdersExist_ReturnsMappedOrderList()
    {
        // Arrange
        const int appUserId = 7;

        var customer = new Customer
        {
            CustomerId = 25,
            FirstName = "İsmet",
            LastName = "Akpınar",
            AppUserId = appUserId
        };

        var firstOrderDate =
            DateTime.UtcNow.AddDays(-2);

        var secondOrderDate =
            DateTime.UtcNow.AddDays(-1);

        var orders = new List<Order>
        {
            new()
            {
                OrderId = 100,
                OrderDate = firstOrderDate,
                Status = OrderStatus.Pending,
                CustomerId = customer.CustomerId
            },
            new()
            {
                OrderId = 101,
                OrderDate = secondOrderDate,
                Status = OrderStatus.Shipped,
                CustomerId = customer.CustomerId
            }
        };

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerServiceMock
            .Setup(x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync(
                It.IsAny<ISpecification<Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result =
            await _orderService.GetMyOrdersAsync();

        // Assert
        result.Should().HaveCount(2);

        result[0].OrderId.Should().Be(100);
        result[0].OrderDate.Should().Be(firstOrderDate);
        result[0].Status.Should().Be(OrderStatus.Pending);
        result[0].CustomerName.Should().Be("İsmet Akpınar");

        result[1].OrderId.Should().Be(101);
        result[1].OrderDate.Should().Be(secondOrderDate);
        result[1].Status.Should().Be(OrderStatus.Shipped);
        result[1].CustomerName.Should().Be("İsmet Akpınar");

        _currentUserServiceMock.Verify(
            x => x.GetAppUserId(),
            Times.Once);

        _customerServiceMock.Verify(
            x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _orderRepositoryMock.Verify(
            x => x.GetAllAsync(
                It.IsAny<GetMyOrdersSpecification>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMyOrdersAsync_NoOrders_ReturnsEmptyList()
    {
        // Arrange
        const int appUserId = 7;

        var customer = new Customer
        {
            CustomerId = 25,
            FirstName = "İsmet",
            LastName = "Akpınar",
            AppUserId = appUserId
        };

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerServiceMock
            .Setup(x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync(
                It.IsAny<ISpecification<Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        // Act
        var result =
            await _orderService.GetMyOrdersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyOrderDetailAsync_OrderExists_ReturnsOrderDetailDto()
    {
        // Arrange
        const int appUserId = 9;
        const int orderId = 50;

        var customer = new Customer
        {
            CustomerId = 30,
            FirstName = "İsmet",
            LastName = "Akpınar",
            AppUserId = appUserId
        };

        var orderDate =
            DateTime.UtcNow.AddHours(-2);

        var order = new Order
        {
            OrderId = orderId,
            OrderDate = orderDate,
            Status = OrderStatus.Preparing,
            CustomerId = customer.CustomerId,
            OrderDetails = new List<OrderDetail>
            {
                new()
                {
                    ProductId = 1,
                    ProductName = "Klavye",
                    UnitPrice = 750m,
                    Quantity = 2
                },
                new()
                {
                    ProductId = 2,
                    ProductName = "Mouse",
                    UnitPrice = 500m,
                    Quantity = 1
                }
            }
        };

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerServiceMock
            .Setup(x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result =
            await _orderService
                .GetMyOrderDetailAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(orderId);
        result.OrderDate.Should().Be(orderDate);
        result.Status.Should().Be(OrderStatus.Preparing);
        result.CustomerId.Should().Be(customer.CustomerId);
        result.CustomerName.Should().Be("İsmet Akpınar");

        result.Items.Should().HaveCount(2);

        result.Items[0].ProductId.Should().Be(1);
        result.Items[0].ProductName.Should().Be("Klavye");
        result.Items[0].UnitPrice.Should().Be(750m);
        result.Items[0].Quantity.Should().Be(2);
        result.Items[0].LineTotal.Should().Be(1500m);

        result.Items[1].ProductId.Should().Be(2);
        result.Items[1].ProductName.Should().Be("Mouse");
        result.Items[1].UnitPrice.Should().Be(500m);
        result.Items[1].Quantity.Should().Be(1);
        result.Items[1].LineTotal.Should().Be(500m);

        result.TotalAmount.Should().Be(2000m);

        _orderRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<MyOrderDetailSpecification>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMyOrderDetailAsync_OrderDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int appUserId = 9;

        var customer = new Customer
        {
            CustomerId = 30,
            FirstName = "İsmet",
            LastName = "Akpınar",
            AppUserId = appUserId
        };

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerServiceMock
            .Setup(x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        Func<Task> act = async () =>
            await _orderService
                .GetMyOrderDetailAsync(999);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Sipariş bulunamadı.");
    }
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Preparing)]
    [InlineData(OrderStatus.Preparing, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered)]
    public async Task UpdateAsync_ValidStatusTransition_UpdatesOrderAndSavesChanges(
    OrderStatus currentStatus,
    OrderStatus newStatus)
    {
        // Arrange
        var order = new Order
        {
            OrderId = 60,
            Status = currentStatus,
            OrderDate = DateTime.UtcNow,
            CustomerId = 10
        };

        var dto = new UpdateOrderDto
        {
            OrderId = order.OrderId,
            Status = newStatus
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(
                dto.OrderId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        await _orderService.UpdateAsync(dto);

        // Assert
        order.Status.Should().Be(newStatus);

        _orderRepositoryMock.Verify(
            x => x.Update(order),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_OrderDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var dto = new UpdateOrderDto
        {
            OrderId = 999,
            Status = OrderStatus.Preparing
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(
                dto.OrderId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        Func<Task> act = async () =>
            await _orderService.UpdateAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Sipariş bulunamadı.");

        _orderRepositoryMock.Verify(
            x => x.Update(It.IsAny<Order>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Pending, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Preparing, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Preparing)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Pending)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Preparing)]
    public async Task UpdateAsync_InvalidStatusTransition_ThrowsBadRequestException(
        OrderStatus currentStatus,
        OrderStatus newStatus)
    {
        // Arrange
        var order = new Order
        {
            OrderId = 70,
            Status = currentStatus,
            OrderDate = DateTime.UtcNow,
            CustomerId = 10
        };

        var dto = new UpdateOrderDto
        {
            OrderId = order.OrderId,
            Status = newStatus
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(
                dto.OrderId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        Func<Task> act = async () =>
            await _orderService.UpdateAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                $"Sipariş durumu {currentStatus} durumundan {newStatus} durumuna geçirilemez.");

        order.Status.Should().Be(currentStatus);

        _orderRepositoryMock.Verify(
            x => x.Update(It.IsAny<Order>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Preparing)]
    public async Task CancelOrderAsync_CancellableOrder_RestoresStockAndCancelsOrder(
        OrderStatus currentStatus)
    {
        // Arrange
        var firstProduct = new Product
        {
            ProductId = 1,
            Name = "Klavye",
            Stock = 10,
            Price = 750m
        };

        var secondProduct = new Product
        {
            ProductId = 2,
            Name = "Mouse",
            Stock = 5,
            Price = 500m
        };

        var order = new Order
        {
            OrderId = 80,
            Status = currentStatus,
            CustomerId = 12,
            OrderDate = DateTime.UtcNow,
            OrderDetails = new List<OrderDetail>
            {
                new()
                {
                    OrderDetailId = 1,
                    ProductId = firstProduct.ProductId,
                    Product = firstProduct,
                    Quantity = 3,
                    UnitPrice = firstProduct.Price
                },
                new()
                {
                    OrderDetailId = 2,
                    ProductId = secondProduct.ProductId,
                    Product = secondProduct,
                    Quantity = 2,
                    UnitPrice = secondProduct.Price
                }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        await _orderService
            .CancelOrderAsync(order.OrderId);

        // Assert
        firstProduct.Stock.Should().Be(13);
        secondProduct.Stock.Should().Be(7);
        order.Status.Should().Be(OrderStatus.Cancelled);

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _productRepositoryMock.Verify(
            x => x.Update(firstProduct),
            Times.Once);

        _productRepositoryMock.Verify(
            x => x.Update(secondProduct),
            Times.Once);

        _orderRepositoryMock.Verify(
            x => x.Update(order),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CancelOrderAsync_OrderDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _orderRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        Func<Task> act = async () =>
            await _orderService.CancelOrderAsync(999);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Sipariş bulunamadı.");

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task CancelOrderAsync_OrderCannotBeCancelled_ThrowsBadRequestException(
        OrderStatus status)
    {
        // Arrange
        var product = new Product
        {
            ProductId = 1,
            Name = "Klavye",
            Stock = 10,
            Price = 750m
        };

        var order = new Order
        {
            OrderId = 90,
            Status = status,
            CustomerId = 15,
            OrderDate = DateTime.UtcNow,
            OrderDetails = new List<OrderDetail>
            {
                new()
                {
                    ProductId = product.ProductId,
                    Product = product,
                    Quantity = 2,
                    UnitPrice = product.Price
                }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        Func<Task> act = async () =>
            await _orderService
                .CancelOrderAsync(order.OrderId);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                $"{status} durumundaki sipariş iptal edilemez.");

        product.Stock.Should().Be(10);
        order.Status.Should().Be(status);

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _productRepositoryMock.Verify(
            x => x.Update(It.IsAny<Product>()),
            Times.Never);

        _orderRepositoryMock.Verify(
            x => x.Update(It.IsAny<Order>()),
            Times.Never);
    }

    [Fact]
    public async Task CancelOrderAsync_SaveChangesThrowsConcurrencyException_RollsBackAndThrowsBadRequestException()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 1,
            Name = "Klavye",
            Stock = 10,
            Price = 750m
        };

        var order = new Order
        {
            OrderId = 100,
            Status = OrderStatus.Pending,
            CustomerId = 20,
            OrderDate = DateTime.UtcNow,
            OrderDetails = new List<OrderDetail>
            {
                new()
                {
                    ProductId = product.ProductId,
                    Product = product,
                    Quantity = 2,
                    UnitPrice = product.Price
                }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new DbUpdateConcurrencyException());

        // Act
        Func<Task> act = async () =>
            await _orderService
                .CancelOrderAsync(order.OrderId);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "Stok bilgisi başka bir işlem tarafından değiştirildi. Sipariş iptal işlemini tekrar deneyin.");

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                CancellationToken.None),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CancelOrderAsync_UnexpectedException_RollsBackAndRethrowsException()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 1,
            Name = "Klavye",
            Stock = 10,
            Price = 750m
        };

        var order = new Order
        {
            OrderId = 110,
            Status = OrderStatus.Preparing,
            CustomerId = 22,
            OrderDate = DateTime.UtcNow,
            OrderDetails = new List<OrderDetail>
            {
                new()
                {
                    ProductId = product.ProductId,
                    Product = product,
                    Quantity = 1,
                    UnitPrice = product.Price
                }
            }
        };

        var expectedException =
            new InvalidOperationException(
                "Beklenmeyen veritabanı hatası.");

        _orderRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        Func<Task> act = async () =>
            await _orderService
                .CancelOrderAsync(order.OrderId);

        // Assert
        var exception = await act.Should()
            .ThrowAsync<InvalidOperationException>();

        exception.Which.Should()
            .BeSameAs(expectedException);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                CancellationToken.None),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}