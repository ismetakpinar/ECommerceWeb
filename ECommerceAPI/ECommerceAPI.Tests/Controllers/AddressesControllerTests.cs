using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Controllers;
using ECommerceAPI.Dto.Dtos.AddressDtos;
using ECommerceAPI.Entity.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ECommerceAPI.Tests.Controllers;

public class AddressesControllerTests
{
    private readonly Mock<IAddressService> _addressServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly AddressesController _controller;

    public AddressesControllerTests()
    {
        _addressServiceMock = new Mock<IAddressService>();
        _mapperMock = new Mock<IMapper>();

        _controller = new AddressesController(
            _addressServiceMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        var addresses = new List<Address>
        {
            new()
            {
                AddressId = 1,
                Title = "Ev",
                City = "Rize",
                District = "Merkez",
                FullAddress = "Adres 1",
                CustomerId = 10
            }
        };

        var dtoList = new List<AddressListDto>
        {
            new()
            {
                AddressId = 1,
                Title = "Ev",
                City = "Rize",
                District = "Merkez",
                CustomerName = "İsmet Akpınar"
            }
        };

        _addressServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(addresses);

        _mapperMock
            .Setup(x => x.Map<List<AddressListDto>>(addresses))
            .Returns(dtoList);

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();

        _addressServiceMock.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<List<AddressListDto>>(addresses),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        var address = new Address
        {
            AddressId = 5,
            Title = "İş",
            City = "Ankara",
            District = "Çankaya",
            FullAddress = "Test",
            CustomerId = 3
        };

        var dto = new AddressDetailDto
        {
            AddressId = 5,
            Title = "İş",
            City = "Ankara",
            District = "Çankaya",
            FullAddress = "Test",
            CustomerId = 3,
            CustomerName = "Ali Veli"
        };

        _addressServiceMock
            .Setup(x => x.GetByIdAsync(
                5,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        _mapperMock
            .Setup(x => x.Map<AddressDetailDto>(address))
            .Returns(dto);

        var result = await _controller.GetById(5);

        result.Should().BeOfType<OkObjectResult>();

        _addressServiceMock.Verify(
            x => x.GetByIdAsync(
                5,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<AddressDetailDto>(address),
            Times.Once);
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var dto = new CreateAddressDto
        {
            Title = "Ev",
            City = "Rize",
            District = "Merkez",
            FullAddress = "Adres"
        };

        var result = await _controller.Create(dto);

        result.Should().BeOfType<ObjectResult>();

        var created = result as ObjectResult;

        created!.StatusCode.Should().Be(201);

        _addressServiceMock.Verify(
            x => x.CreateAsync(
                dto,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task Update_ReturnsOk()
    {
        var dto = new UpdateAddressDto
        {
            AddressId = 5,
            Title = "Ev",
            City = "Rize",
            District = "Merkez",
            FullAddress = "Yeni Adres"
        };

        var result = await _controller.Update(dto);

        result.Should().BeOfType<OkObjectResult>();

        _addressServiceMock.Verify(
            x => x.UpdateAsync(
                dto,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsOk()
    {
        var result = await _controller.Delete(5);

        result.Should().BeOfType<OkObjectResult>();

        _addressServiceMock.Verify(
            x => x.DeleteAsync(
                5,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_ReturnsCorrectApiResponse()
    {
        _addressServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Address>());

        _mapperMock
            .Setup(x => x.Map<List<AddressListDto>>(It.IsAny<List<Address>>()))
            .Returns(new List<AddressListDto>());

        var result = await _controller.GetAll();

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<List<AddressListDto>>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.StatusCode.Should().Be(200);
        response.Message.Should().Be("Adresler başarıyla listelendi.");
    }

    [Fact]
    public async Task GetById_ReturnsCorrectApiResponse()
    {
        var address = new Address
        {
            AddressId = 3
        };

        _addressServiceMock
            .Setup(x => x.GetByIdAsync(
                3,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        _mapperMock
            .Setup(x => x.Map<AddressDetailDto>(address))
            .Returns(new AddressDetailDto
            {
                AddressId = 3,
                Title = "Ev",
                City = "Rize",
                District = "Merkez",
                FullAddress = "Adres",
                CustomerId = 1,
                CustomerName = "İsmet Akpınar"
            });

        var result = await _controller.GetById(3);

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<AddressDetailDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data!.AddressId.Should().Be(3);
    }

    [Fact]
    public async Task Create_ReturnsCorrectApiResponse()
    {
        var dto = new CreateAddressDto
        {
            Title = "Ev",
            City = "Rize",
            District = "Merkez",
            FullAddress = "Adres"
        };

        var result = await _controller.Create(dto);

        var created = result as ObjectResult;
        var response = created!.Value as ApiResponse<object>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.StatusCode.Should().Be(201);
        response.Message.Should().Be("Adres başarıyla oluşturuldu.");
    }

    [Fact]
    public async Task Update_ReturnsCorrectApiResponse()
    {
        var dto = new UpdateAddressDto
        {
            AddressId = 1,
            Title = "Ev",
            City = "Rize",
            District = "Merkez",
            FullAddress = "Yeni Adres"
        };

        var result = await _controller.Update(dto);

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<object>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Adres başarıyla güncellendi.");
    }

    [Fact]
    public async Task Delete_ReturnsCorrectApiResponse()
    {
        var result = await _controller.Delete(1);

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<object>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Adres başarıyla silindi.");
    }
}