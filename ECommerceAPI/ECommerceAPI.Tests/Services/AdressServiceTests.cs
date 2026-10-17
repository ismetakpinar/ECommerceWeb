using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Concrete;
using ECommerceAPI.DataAccess.Specifications;
using ECommerceAPI.DataAccess.UnitOfWorks;
using ECommerceAPI.Dto.Dtos.AddressDtos;
using ECommerceAPI.Entity.Entities;
using ECommerceAPI.Entity.Exceptions;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;

namespace ECommerceAPI.Tests.Services;

public class AddressServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICustomerService> _customerServiceMock;
    private readonly Mock<IGenericRepository<Address>> _addressRepositoryMock;

    private readonly AddressService _addressService;

    public AddressServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _customerServiceMock = new Mock<ICustomerService>();

        _addressRepositoryMock =
            new Mock<IGenericRepository<Address>>();

        _unitOfWorkMock
            .Setup(x => x.Repository<Address>())
            .Returns(_addressRepositoryMock.Object);

        _addressService = new AddressService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object,
            _customerServiceMock.Object);
    }

    [Fact]
    public async Task CreateAsync_AddressDoesNotExist_AddsAddressAndSavesChanges()
    {
        // Arrange
        const int appUserId = 10;
        const int customerId = 20;

        var dto = new CreateAddressDto
        {
            Title = "  Ev Adresi  ",
            City = "  İzmir  ",
            District = "  Buca  ",
            FullAddress = "  Örnek Mahallesi No: 10  "
        };

        var customer = new Customer
        {
            CustomerId = customerId,
            AppUserId = appUserId,
            FirstName = "İsmet",
            LastName = "Akpınar",
            Phone = "05000000000"
        };

        var mappedAddress = new Address
        {
            AddressId = 1,
            Title = "Ev Adresi",
            City = "İzmir",
            District = "Buca",
            FullAddress = "Örnek Mahallesi No: 10"
        };

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerServiceMock
            .Setup(x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _addressRepositoryMock
            .Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mapperMock
            .Setup(x => x.Map<Address>(dto))
            .Returns(mappedAddress);

        // Act
        await _addressService.CreateAsync(dto);

        // Assert
        dto.Title.Should().Be("Ev Adresi");
        dto.City.Should().Be("İzmir");
        dto.District.Should().Be("Buca");
        dto.FullAddress.Should()
            .Be("Örnek Mahallesi No: 10");

        mappedAddress.CustomerId.Should().Be(customerId);

        _currentUserServiceMock.Verify(
            x => x.GetAppUserId(),
            Times.Once);

        _customerServiceMock.Verify(
            x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _addressRepositoryMock.Verify(
            x => x.AnyAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<Address>(dto),
            Times.Once);

        _addressRepositoryMock.Verify(
            x => x.AddAsync(
                mappedAddress,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_AddressAlreadyExists_ThrowsBadRequestException()
    {
        // Arrange
        const int appUserId = 10;
        const int customerId = 20;

        var dto = new CreateAddressDto
        {
            Title = "  Ev Adresi  ",
            City = "  İzmir  ",
            District = "  Buca  ",
            FullAddress = "  Örnek Mahallesi No: 10  "
        };

        var customer = new Customer
        {
            CustomerId = customerId,
            AppUserId = appUserId,
            FirstName = "İsmet",
            LastName = "Akpınar",
            Phone = "05000000000"
        };

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerServiceMock
            .Setup(x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _addressRepositoryMock
            .Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () =>
            await _addressService.CreateAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Bu adres zaten kayıtlı.");

        dto.Title.Should().Be("Ev Adresi");
        dto.City.Should().Be("İzmir");
        dto.District.Should().Be("Buca");
        dto.FullAddress.Should()
            .Be("Örnek Mahallesi No: 10");

        _mapperMock.Verify(
            x => x.Map<Address>(
                It.IsAny<CreateAddressDto>()),
            Times.Never);

        _addressRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Address>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_CurrentCustomerHasAddresses_ReturnsAddresses()
    {
        // Arrange
        const int appUserId = 10;
        const int customerId = 20;

        var customer = new Customer
        {
            CustomerId = customerId,
            AppUserId = appUserId,
            FirstName = "İsmet",
            LastName = "Akpınar",
            Phone = "05000000000"
        };

        var addresses = new List<Address>
        {
            new()
            {
                AddressId = 1,
                CustomerId = customerId,
                Title = "Ev",
                City = "İzmir",
                District = "Buca",
                FullAddress = "Örnek Mahallesi No: 10"
            },
            new()
            {
                AddressId = 2,
                CustomerId = customerId,
                Title = "İş",
                City = "İzmir",
                District = "Konak",
                FullAddress = "Örnek Caddesi No: 20"
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

        _addressRepositoryMock
            .Setup(x => x.GetAllAsync(
                It.IsAny<ISpecification<Address>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(addresses);

        // Act
        var result = await _addressService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(addresses);

        result.Should()
            .OnlyContain(x => x.CustomerId == customerId);

        _currentUserServiceMock.Verify(
            x => x.GetAppUserId(),
            Times.Once);

        _customerServiceMock.Verify(
            x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _addressRepositoryMock.Verify(
            x => x.GetAllAsync(
                It.IsAny<ISpecification<Address>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_CurrentCustomerHasNoAddresses_ReturnsEmptyList()
    {
        // Arrange
        const int appUserId = 10;
        const int customerId = 20;

        var customer = new Customer
        {
            CustomerId = customerId,
            AppUserId = appUserId,
            FirstName = "İsmet",
            LastName = "Akpınar",
            Phone = "05000000000"
        };

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerServiceMock
            .Setup(x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _addressRepositoryMock
            .Setup(x => x.GetAllAsync(
                It.IsAny<ISpecification<Address>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Address>());

        // Act
        var result = await _addressService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _addressRepositoryMock.Verify(
            x => x.GetAllAsync(
                It.IsAny<ISpecification<Address>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_AddressBelongsToCurrentCustomer_ReturnsAddress()
    {
        // Arrange
        const int appUserId = 10;
        const int customerId = 20;
        const int addressId = 1;

        var customer = new Customer
        {
            CustomerId = customerId,
            AppUserId = appUserId,
            FirstName = "İsmet",
            LastName = "Akpınar",
            Phone = "05000000000"
        };

        var address = new Address
        {
            AddressId = addressId,
            CustomerId = customerId,
            Title = "Ev",
            City = "İzmir",
            District = "Buca",
            FullAddress = "Örnek Mahallesi No: 10"
        };

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerServiceMock
            .Setup(x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _addressRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        // Act
        var result =
            await _addressService.GetByIdAsync(addressId);

        // Assert
        result.Should().NotBeNull();
        result.AddressId.Should().Be(addressId);
        result.CustomerId.Should().Be(customerId);
        result.Should().BeSameAs(address);

        _currentUserServiceMock.Verify(
            x => x.GetAppUserId(),
            Times.Once);

        _customerServiceMock.Verify(
            x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _addressRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_AddressDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int appUserId = 10;
        const int customerId = 20;
        const int addressId = 999;

        var customer = new Customer
        {
            CustomerId = customerId,
            AppUserId = appUserId,
            FirstName = "İsmet",
            LastName = "Akpınar",
            Phone = "05000000000"
        };

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerServiceMock
            .Setup(x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _addressRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Address?)null);

        // Act
        Func<Task> act = async () =>
            await _addressService.GetByIdAsync(addressId);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Adres bulunamadı.");

        _addressRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task UpdateAsync_AddressExists_UpdatesAddressAndSavesChanges()
    {
        // Arrange
        const int appUserId = 10;
        const int customerId = 20;

        var dto = new UpdateAddressDto
        {
            AddressId = 1,
            Title = "  Ev  ",
            City = "  İzmir  ",
            District = "  Buca  ",
            FullAddress = "  123 Sokak No:10  "
        };

        var customer = new Customer
        {
            CustomerId = customerId,
            AppUserId = appUserId
        };

        var address = new Address
        {
            AddressId = dto.AddressId,
            CustomerId = customerId,
            Title = "Eski",
            City = "Eski",
            District = "Eski",
            FullAddress = "Eski"
        };

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerServiceMock
            .Setup(x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _addressRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        _addressRepositoryMock
            .Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mapperMock
            .Setup(x => x.Map(dto, address))
            .Callback(() =>
            {
                address.Title = dto.Title;
                address.City = dto.City;
                address.District = dto.District;
                address.FullAddress = dto.FullAddress;
            });

        // Act
        await _addressService.UpdateAsync(dto);

        // Assert
        dto.Title.Should().Be("Ev");
        dto.City.Should().Be("İzmir");
        dto.District.Should().Be("Buca");
        dto.FullAddress.Should().Be("123 Sokak No:10");

        address.Title.Should().Be("Ev");
        address.City.Should().Be("İzmir");
        address.District.Should().Be("Buca");
        address.FullAddress.Should().Be("123 Sokak No:10");

        _mapperMock.Verify(
            x => x.Map(dto, address),
            Times.Once);

        _addressRepositoryMock.Verify(
            x => x.Update(address),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_AddressNotFound_ThrowsNotFoundException()
    {
        // Arrange
        const int appUserId = 10;
        const int customerId = 20;

        var dto = new UpdateAddressDto
        {
            AddressId = 999,
            Title = "Ev",
            City = "İzmir",
            District = "Buca",
            FullAddress = "123 Sokak"
        };

        var customer = new Customer
        {
            CustomerId = customerId,
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

        _addressRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Address?)null);

        // Act
        Func<Task> act = async () =>
            await _addressService.UpdateAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Adres bulunamadı.");

        _addressRepositoryMock.Verify(
            x => x.Update(It.IsAny<Address>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateAddress_ThrowsBadRequestException()
    {
        // Arrange
        const int appUserId = 10;
        const int customerId = 20;

        var dto = new UpdateAddressDto
        {
            AddressId = 1,
            Title = "Ev",
            City = "İzmir",
            District = "Buca",
            FullAddress = "123 Sokak"
        };

        var customer = new Customer
        {
            CustomerId = customerId,
            AppUserId = appUserId
        };

        var address = new Address
        {
            AddressId = 1,
            CustomerId = customerId
        };

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerServiceMock
            .Setup(x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _addressRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        _addressRepositoryMock
            .Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () =>
            await _addressService.UpdateAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Bu adres zaten kayıtlı.");

        _addressRepositoryMock.Verify(
            x => x.Update(It.IsAny<Address>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_AddressExists_DeletesAddressAndSavesChanges()
    {
        // Arrange
        const int appUserId = 10;
        const int customerId = 20;

        var customer = new Customer
        {
            CustomerId = customerId,
            AppUserId = appUserId
        };

        var address = new Address
        {
            AddressId = 1,
            CustomerId = customerId
        };

        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerServiceMock
            .Setup(x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _addressRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        // Act
        await _addressService.DeleteAsync(address.AddressId);

        // Assert
        _addressRepositoryMock.Verify(
            x => x.SoftDelete(address),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_AddressNotFound_ThrowsNotFoundException()
    {
        // Arrange
        const int appUserId = 10;
        const int customerId = 20;

        var customer = new Customer
        {
            CustomerId = customerId,
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

        _addressRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Address?)null);

        // Act
        Func<Task> act = async () =>
            await _addressService.DeleteAsync(999);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Adres bulunamadı.");

        _addressRepositoryMock.Verify(
            x => x.SoftDelete(It.IsAny<Address>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
