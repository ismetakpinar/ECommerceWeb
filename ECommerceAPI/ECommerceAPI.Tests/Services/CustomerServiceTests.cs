using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Concrete;
using ECommerceAPI.DataAccess.Specifications;
using ECommerceAPI.DataAccess.UnitOfWorks;
using ECommerceAPI.Dto.Dtos.CustomerDtos;
using ECommerceAPI.Entity.Entities;
using ECommerceAPI.Entity.Exceptions;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;

namespace ECommerceAPI.Tests.Services;

public class CustomerServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    private readonly Mock<IGenericRepository<Customer>>
        _customerRepositoryMock;

    private readonly Mock<IGenericRepository<Order>>
        _orderRepositoryMock;

    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _currentUserServiceMock =
            new Mock<ICurrentUserService>();

        _customerRepositoryMock =
            new Mock<IGenericRepository<Customer>>();

        _orderRepositoryMock =
            new Mock<IGenericRepository<Order>>();

        _unitOfWorkMock
            .Setup(x => x.Repository<Customer>())
            .Returns(_customerRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.Repository<Order>())
            .Returns(_orderRepositoryMock.Object);

        _customerService = new CustomerService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_CustomersExist_ReturnsCustomers()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new()
            {
                CustomerId = 1,
                FirstName = "İsmet",
                LastName = "Akpınar",
                Phone = "05000000001",
                AppUserId = 10
            },
            new()
            {
                CustomerId = 2,
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                Phone = "05000000002",
                AppUserId = 11
            }
        };

        _customerRepositoryMock
            .Setup(x => x.GetAllAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        // Act
        var result = await _customerService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(customers);

        _customerRepositoryMock.Verify(
            x => x.GetAllAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_CustomerExists_ReturnsCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 1,
            FirstName = "İsmet",
            LastName = "Akpınar",
            Phone = "05000000001",
            AppUserId = 10
        };

        _customerRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Customer>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _customerService.GetByIdAsync(
            customer.CustomerId);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should()
            .Be(customer.CustomerId);

        result.FirstName.Should()
            .Be(customer.FirstName);

        result.LastName.Should()
            .Be(customer.LastName);

        _customerRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<ISpecification<Customer>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_CustomerDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int customerId = 999;

        _customerRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Customer>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        Func<Task> act = async () =>
            await _customerService.GetByIdAsync(customerId);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Müşteri bulunamadı.");
    }

    [Fact]
    public async Task GetByAppUserIdAsync_CustomerExists_ReturnsCustomer()
    {
        // Arrange
        const int appUserId = 10;

        var customer = new Customer
        {
            CustomerId = 1,
            FirstName = "İsmet",
            LastName = "Akpınar",
            Phone = "05000000001",
            AppUserId = appUserId
        };

        _customerRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result =
            await _customerService.GetByAppUserIdAsync(
                appUserId);

        // Assert
        result.Should().NotBeNull();
        result.AppUserId.Should().Be(appUserId);
        result.CustomerId.Should()
            .Be(customer.CustomerId);

        _customerRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<Expression<
                    Func<Customer, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByAppUserIdAsync_CustomerDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int appUserId = 999;

        _customerRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        Func<Task> act = async () =>
            await _customerService
                .GetByAppUserIdAsync(appUserId);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage(
                "Kullanıcıya ait müşteri kaydı bulunamadı.");
    }

    [Fact]
    public async Task GetMeAsync_CurrentUserHasCustomer_ReturnsCustomer()
    {
        // Arrange
        const int appUserId = 10;

        var customer = new Customer
        {
            CustomerId = 1,
            FirstName = "İsmet",
            LastName = "Akpınar",
            Phone = "05000000001",
            AppUserId = appUserId
        };

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerRepositoryMock
    .Setup(x => x.GetAsync(
        It.IsAny<ISpecification<Customer>>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(customer);

        // Act
        var result = await _customerService.GetMeAsync();

        // Assert
        result.Should().NotBeNull();
        result.AppUserId.Should().Be(appUserId);
        result.CustomerId.Should()
            .Be(customer.CustomerId);

        _currentUserServiceMock.Verify(
            x => x.GetAppUserId(),
            Times.Once);

        _customerRepositoryMock.Verify(
    x => x.GetAsync(
        It.IsAny<ISpecification<Customer>>(),
        It.IsAny<CancellationToken>()),
    Times.Once);
    }

    [Fact]
    public async Task GetMeAsync_CurrentUserHasNoCustomer_ThrowsNotFoundException()
    {
        // Arrange
        const int appUserId = 999;

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerRepositoryMock
    .Setup(x => x.GetAsync(
        It.IsAny<ISpecification<Customer>>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync((Customer?)null);

        // Act
        Func<Task> act = async () =>
            await _customerService.GetMeAsync();

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage(
                "Kullanıcıya ait müşteri kaydı bulunamadı.");

        _currentUserServiceMock.Verify(
            x => x.GetAppUserId(),
            Times.Once);
    }
    [Fact]
    public async Task UpdateAsync_CurrentUserHasCustomer_UpdatesCustomerAndSavesChanges()
    {
        // Arrange
        const int appUserId = 10;

        var dto = new UpdateCustomerDto
        {
            FirstName = "  İsmet  ",
            LastName = "  Akpınar  ",
            Phone = "  05000000001  "
        };

        var customer = new Customer
        {
            CustomerId = 1,
            FirstName = "Eski ad",
            LastName = "Eski soyad",
            Phone = "05000000000",
            AppUserId = appUserId
        };

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mapperMock
            .Setup(x => x.Map<UpdateCustomerDto, Customer>(
                dto,
                customer))
            .Callback<UpdateCustomerDto, Customer>(
                (source, destination) =>
                {
                    destination.FirstName =
                        source.FirstName;

                    destination.LastName =
                        source.LastName;

                    destination.Phone =
                        source.Phone;
                })
            .Returns(customer);

        // Act
        await _customerService.UpdateAsync(dto);

        // Assert
        dto.FirstName.Should().Be("İsmet");
        dto.LastName.Should().Be("Akpınar");
        dto.Phone.Should().Be("05000000001");

        customer.FirstName.Should().Be("İsmet");
        customer.LastName.Should().Be("Akpınar");
        customer.Phone.Should().Be("05000000001");

        _currentUserServiceMock.Verify(
            x => x.GetAppUserId(),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<UpdateCustomerDto, Customer>(
                dto,
                customer),
            Times.Once);

        _customerRepositoryMock.Verify(
            x => x.Update(customer),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CurrentUserHasNoCustomer_ThrowsNotFoundException()
    {
        // Arrange
        const int appUserId = 999;

        var dto = new UpdateCustomerDto
        {
            FirstName = "  İsmet  ",
            LastName = "  Akpınar  ",
            Phone = "  05000000001  "
        };

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        Func<Task> act = async () =>
            await _customerService.UpdateAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage(
                "Kullanıcıya ait müşteri kaydı bulunamadı.");

        dto.FirstName.Should().Be("İsmet");
        dto.LastName.Should().Be("Akpınar");
        dto.Phone.Should().Be("05000000001");

        _mapperMock.Verify(
            x => x.Map<UpdateCustomerDto, Customer>(
                It.IsAny<UpdateCustomerDto>(),
                It.IsAny<Customer>()),
            Times.Never);

        _customerRepositoryMock.Verify(
            x => x.Update(It.IsAny<Customer>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_CustomerExistsAndHasNoOrders_SoftDeletesCustomerAndSavesChanges()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 1,
            FirstName = "İsmet",
            LastName = "Akpınar",
            Phone = "05000000001",
            AppUserId = 10
        };

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(
                customer.CustomerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _customerService.DeleteAsync(
            customer.CustomerId);

        // Assert
        _customerRepositoryMock.Verify(
            x => x.GetByIdAsync(
                customer.CustomerId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _orderRepositoryMock.Verify(
            x => x.AnyAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _customerRepositoryMock.Verify(
            x => x.SoftDelete(customer),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CustomerDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int customerId = 999;

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        Func<Task> act = async () =>
            await _customerService.DeleteAsync(customerId);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Müşteri bulunamadı.");

        _orderRepositoryMock.Verify(
            x => x.AnyAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _customerRepositoryMock.Verify(
            x => x.SoftDelete(It.IsAny<Customer>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_CustomerHasOrders_ThrowsBadRequestException()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 1,
            FirstName = "İsmet",
            LastName = "Akpınar",
            Phone = "05000000001",
            AppUserId = 10
        };

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(
                customer.CustomerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () =>
            await _customerService.DeleteAsync(
                customer.CustomerId);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "Siparişi bulunan müşteri silinemez.");

        _orderRepositoryMock.Verify(
            x => x.AnyAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _customerRepositoryMock.Verify(
            x => x.SoftDelete(It.IsAny<Customer>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

}