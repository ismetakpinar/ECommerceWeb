using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Controllers;
using ECommerceAPI.Dto.Dtos.CustomerDtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ECommerceAPI.Tests.Controllers;

public class CustomersControllerTests
{
    private readonly Mock<ICustomerService> _customerServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CustomersController _controller;

    public CustomersControllerTests()
    {
        _customerServiceMock = new Mock<ICustomerService>();
        _mapperMock = new Mock<IMapper>();

        _controller = new CustomersController(
            _customerServiceMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        var customers = new List<Customer>
        {
            new()
            {
                CustomerId = 1,
                FirstName = "İsmet",
                LastName = "Akpınar",
                Phone = "05050000000"
            }
        };

        var dtoList = new List<CustomerListDto>
        {
            new()
            {
                CustomerId = 1,
                FirstName = "İsmet",
                LastName = "Akpınar",
                Email = "test@test.com",
                Phone = "05050000000"
            }
        };

        _customerServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        _mapperMock
            .Setup(x => x.Map<List<CustomerListDto>>(customers))
            .Returns(dtoList);

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();

        _customerServiceMock.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<List<CustomerListDto>>(customers),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        var customer = new Customer
        {
            CustomerId = 5,
            FirstName = "Ali",
            LastName = "Yılmaz",
            Phone = "05555555555"
        };

        var dto = new CustomerDetailDto
        {
            CustomerId = 5,
            FirstName = "Ali",
            LastName = "Yılmaz",
            Email = "ali@test.com",
            Phone = "05555555555",
            AddressCount = 2,
            OrderCount = 3
        };

        _customerServiceMock
            .Setup(x => x.GetByIdAsync(
                5,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mapperMock
            .Setup(x => x.Map<CustomerDetailDto>(customer))
            .Returns(dto);

        var result = await _controller.GetById(5);

        result.Should().BeOfType<OkObjectResult>();

        _customerServiceMock.Verify(
            x => x.GetByIdAsync(
                5,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<CustomerDetailDto>(customer),
            Times.Once);
    }

    [Fact]
    public async Task GetMe_ReturnsOkResult()
    {
        var customer = new Customer
        {
            CustomerId = 10,
            FirstName = "Mehmet",
            LastName = "Demir",
            Phone = "05444444444"
        };

        var dto = new CustomerDetailDto
        {
            CustomerId = 10,
            FirstName = "Mehmet",
            LastName = "Demir",
            Email = "mehmet@test.com",
            Phone = "05444444444",
            AddressCount = 1,
            OrderCount = 5
        };

        _customerServiceMock
            .Setup(x => x.GetMeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mapperMock
            .Setup(x => x.Map<CustomerDetailDto>(customer))
            .Returns(dto);

        var result = await _controller.GetMe();

        result.Should().BeOfType<OkObjectResult>();

        _customerServiceMock.Verify(
            x => x.GetMeAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<CustomerDetailDto>(customer),
            Times.Once);
    }
    [Fact]
    public async Task UpdateMe_ReturnsOk()
    {
        // Arrange
        var dto = new UpdateCustomerDto
        {
            FirstName = "Yeni",
            LastName = "İsim",
            Phone = "05555555555"
        };

        // Act
        var result = await _controller.UpdateMe(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _customerServiceMock.Verify(
            x => x.UpdateAsync(
                dto,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsOk()
    {
        // Act
        var result = await _controller.Delete(7);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _customerServiceMock.Verify(
            x => x.DeleteAsync(
                7,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_ReturnsCorrectApiResponse()
    {
        // Arrange
        _customerServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer>());

        _mapperMock
            .Setup(x => x.Map<List<CustomerListDto>>(It.IsAny<List<Customer>>()))
            .Returns(new List<CustomerListDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<List<CustomerListDto>>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.StatusCode.Should().Be(200);
        response.Message.Should().Be("Müşteriler başarıyla listelendi.");
    }

    [Fact]
    public async Task GetById_ReturnsCorrectApiResponse()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 3
        };

        _customerServiceMock
            .Setup(x => x.GetByIdAsync(
                3,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mapperMock
            .Setup(x => x.Map<CustomerDetailDto>(customer))
            .Returns(new CustomerDetailDto
            {
                CustomerId = 3,
                FirstName = "Ali",
                LastName = "Veli",
                Email = "ali@test.com",
                Phone = "05555555555",
                AddressCount = 2,
                OrderCount = 4
            });

        // Act
        var result = await _controller.GetById(3);

        // Assert
        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<CustomerDetailDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data!.CustomerId.Should().Be(3);
    }

    [Fact]
    public async Task GetMe_ReturnsCorrectApiResponse()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 8
        };

        _customerServiceMock
            .Setup(x => x.GetMeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mapperMock
            .Setup(x => x.Map<CustomerDetailDto>(customer))
            .Returns(new CustomerDetailDto
            {
                CustomerId = 8,
                FirstName = "İsmet",
                LastName = "Akpınar",
                Email = "ismet@test.com",
                Phone = "05555555555",
                AddressCount = 1,
                OrderCount = 6
            });

        // Act
        var result = await _controller.GetMe();

        // Assert
        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<CustomerDetailDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data!.CustomerId.Should().Be(8);
    }

    [Fact]
    public async Task UpdateMe_ReturnsCorrectApiResponse()
    {
        // Arrange
        var dto = new UpdateCustomerDto
        {
            FirstName = "Yeni",
            LastName = "İsim",
            Phone = "05555555555"
        };

        // Act
        var result = await _controller.UpdateMe(dto);

        // Assert
        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<string>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Müşteri başarıyla güncellendi.");
    }

    [Fact]
    public async Task Delete_ReturnsCorrectApiResponse()
    {
        // Act
        var result = await _controller.Delete(5);

        // Assert
        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<string>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Müşteri başarıyla silindi.");
    }
}
