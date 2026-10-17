using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Controllers;
using ECommerceAPI.Dto.Dtos.ProductDtos;
using ECommerceAPI.Dto.Requests;
using ECommerceAPI.Entity.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ECommerceAPI.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _productServiceMock = new Mock<IProductService>();
        _mapperMock = new Mock<IMapper>();

        _controller = new ProductsController(
            _productServiceMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        // Arrange
        var products = new List<Product>
    {
        new()
        {
            ProductId = 1,
            Name = "Laptop",
            Price = 1000
        }
    };

        var pagedResult = new PagedResult<Product>
        {
            Items = products,
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };

        var dtoList = new List<ProductListDto>
    {
        new()
        {
            ProductId = 1,
            Name = "Laptop",
            Price = 1000
        }
    };

        var request = new ProductQueryRequest
        {
            Page = 1,
            PageSize = 10
        };

        _productServiceMock
            .Setup(x => x.GetPagedWithCategoryAsync(
                It.IsAny<ProductQueryRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        _mapperMock
            .Setup(x => x.Map<List<ProductListDto>>(products))
            .Returns(dtoList);

        // Act
        var result = await _controller.GetAll(
            request,
            CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var ok = result as OkObjectResult;

        ok!.Value.Should()
            .BeOfType<ApiResponse<PagedResult<ProductListDto>>>();

        _productServiceMock.Verify(
            x => x.GetPagedWithCategoryAsync(
                It.IsAny<ProductQueryRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<List<ProductListDto>>(products),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 1,
            Name = "Laptop"
        };

        var dto = new ProductDetailDto
        {
            ProductId = 1,
            Name = "Laptop"
        };

        _productServiceMock
            .Setup(x => x.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _mapperMock
            .Setup(x => x.Map<ProductDetailDto>(product))
            .Returns(dto);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _productServiceMock.Verify(
            x => x.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<ProductDetailDto>(product),
            Times.Once);
    }

    [Fact]
    public async Task Create_CallsService_AndReturnsOk()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "Keyboard",
            Price = 500
        };

        var product = new Product
        {
            ProductId = 2,
            Name = "Keyboard",
            Price = 500
        };

        var detailDto = new ProductDetailDto
        {
            ProductId = 2,
            Name = "Keyboard",
            Price = 500
        };

        _mapperMock
            .Setup(x => x.Map<Product>(dto))
            .Returns(product);

        _mapperMock
            .Setup(x => x.Map<ProductDetailDto>(product))
            .Returns(detailDto);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var createdResult = result.Should()
            .BeOfType<CreatedAtActionResult>()
            .Subject;

        createdResult.ActionName.Should()
            .Be(nameof(ProductsController.GetById));

        createdResult.RouteValues.Should().NotBeNull();

        createdResult.RouteValues!["id"]
            .Should().Be(product.ProductId);

        createdResult.RouteValues["version"]
            .Should().Be("1.0");

        var response = createdResult.Value
            .Should()
            .BeOfType<ApiResponse<ProductDetailDto>>()
            .Subject;

        response.Success.Should().BeTrue();
        response.Message.Should().Be("Ürün başarıyla oluşturuldu.");
        response.Data.Should().BeEquivalentTo(detailDto);

        _productServiceMock.Verify(
            x => x.CreateAsync(
                product,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<Product>(dto),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<ProductDetailDto>(product),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_ReturnsCorrectApiResponse()
    {
        // Arrange
        var paged = new PagedResult<Product>
        {
            Items = new(),
            Page = 1,
            PageSize = 10,
            TotalCount = 0
        };

        var request = new ProductQueryRequest
        {
            Page = 1,
            PageSize = 10
        };

        _productServiceMock
            .Setup(x => x.GetPagedWithCategoryAsync(
                It.IsAny<ProductQueryRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        _mapperMock
            .Setup(x => x.Map<List<ProductListDto>>(paged.Items))
            .Returns(new List<ProductListDto>());

        // Act
        var result = await _controller.GetAll(
            request,
            CancellationToken.None);

        // Assert
        var ok = result as OkObjectResult;

        var response =
            ok!.Value as ApiResponse<PagedResult<ProductListDto>>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.StatusCode.Should().Be(200);
        response.Message.Should().Be("Ürünler başarıyla listelendi.");
    }

    [Fact]
    public async Task GetById_ReturnsCorrectApiResponse()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 3,
            Name = "Mouse"
        };

        var dto = new ProductDetailDto
        {
            ProductId = 3,
            Name = "Mouse"
        };

        _productServiceMock
            .Setup(x => x.GetByIdAsync(
                3,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _mapperMock
            .Setup(x => x.Map<ProductDetailDto>(product))
            .Returns(dto);

        // Act
        var result = await _controller.GetById(3);

        // Assert
        var ok = result as OkObjectResult;

        var response =
            ok!.Value as ApiResponse<ProductDetailDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data!.ProductId.Should().Be(3);
        response.Data.Name.Should().Be("Mouse");
    }
    [Fact]
    public async Task Update_CallsService_AndReturnsOk()
    {
        // Arrange
        var dto = new ProductUpdateDto
        {
            Name = "Updated Laptop",
            Price = 2500
        };

        var product = new Product
        {
            ProductId = 1,
            Name = "Laptop",
            Price = 2000
        };

        var detailDto = new ProductDetailDto
        {
            ProductId = 1,
            Name = "Updated Laptop",
            Price = 2500
        };

        _productServiceMock
            .Setup(x => x.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _mapperMock
            .Setup(x => x.Map(dto, product));

        _mapperMock
            .Setup(x => x.Map<ProductDetailDto>(product))
            .Returns(detailDto);

        // Act
        var result = await _controller.Update(1, dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _productServiceMock.Verify(
            x => x.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map(dto, product),
            Times.Once);

        _productServiceMock.Verify(
            x => x.UpdateAsync(
                product,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_CallsService_AndReturnsOk()
    {
        // Act
        var result = await _controller.Delete(5);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _productServiceMock.Verify(
            x => x.DeleteAsync(
                5,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IncreaseStock_CallsService_AndReturnsOk()
    {
        // Arrange
        var dto = new StockUpdateDto
        {
            Quantity = 15
        };

        // Act
        var result =
            await _controller.IncreaseStock(
                10,
                dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _productServiceMock.Verify(
            x => x.IncreaseStockAsync(
                10,
                15,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DecreaseStock_CallsService_AndReturnsOk()
    {
        // Arrange
        var dto = new StockUpdateDto
        {
            Quantity = 8
        };

        // Act
        var result =
            await _controller.DecreaseStock(
                4,
                dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _productServiceMock.Verify(
            x => x.DecreaseStockAsync(
                4,
                8,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_ReturnsCorrectApiResponse()
    {
        // Arrange
        var dto = new ProductUpdateDto
        {
            Name = "Updated",
            Price = 100
        };

        var product = new Product
        {
            ProductId = 11,
            Name = "Old",
            Price = 90
        };

        _productServiceMock
            .Setup(x => x.GetByIdAsync(
                11,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _mapperMock
            .Setup(x => x.Map(dto, product));

        _mapperMock
            .Setup(x => x.Map<ProductDetailDto>(product))
            .Returns(new ProductDetailDto
            {
                ProductId = 11,
                Name = "Updated",
                Price = 100
            });

        // Act
        var result =
            await _controller.Update(
                11,
                dto);

        // Assert
        var ok = result as OkObjectResult;

        var response =
            ok!.Value as ApiResponse<ProductDetailDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should()
            .Be("Ürün başarıyla güncellendi.");
    }

    [Fact]
    public async Task Delete_ReturnsCorrectApiResponse()
    {
        // Act
        var result =
            await _controller.Delete(9);

        // Assert
        var ok =
            result as OkObjectResult;

        var response =
            ok!.Value as ApiResponse<object>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should()
            .Be("Ürün başarıyla silindi.");
    }

    [Fact]
    public async Task IncreaseStock_ReturnsCorrectApiResponse()
    {
        // Arrange
        var dto = new StockUpdateDto
        {
            Quantity = 3
        };

        // Act
        var result =
            await _controller.IncreaseStock(
                2,
                dto);

        // Assert
        var ok =
            result as OkObjectResult;

        var response =
            ok!.Value as ApiResponse<object>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should()
            .Be("Ürün stoğu başarıyla artırıldı.");
    }

    [Fact]
    public async Task DecreaseStock_ReturnsCorrectApiResponse()
    {
        // Arrange
        var dto = new StockUpdateDto
        {
            Quantity = 2
        };

        // Act
        var result =
            await _controller.DecreaseStock(
                2,
                dto);

        // Assert
        var ok =
            result as OkObjectResult;

        var response =
            ok!.Value as ApiResponse<object>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should()
            .Be("Ürün stoğu başarıyla azaltıldı.");
    }
}
