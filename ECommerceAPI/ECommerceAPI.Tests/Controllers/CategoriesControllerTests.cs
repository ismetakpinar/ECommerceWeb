using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Dto.Dtos.CategoryDtos;
using ECommerceAPI.Entity.Entities;
using ECommerceAPI.WebApi.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ECommerceAPI.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _categoryServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _categoryServiceMock = new Mock<ICategoryService>();
        _mapperMock = new Mock<IMapper>();

        _controller = new CategoriesController(
            _categoryServiceMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        // Arrange
        var categories = new List<Category>
        {
            new()
            {
                CategoryId = 1,
                Name = "Elektronik"
            }
        };

        var dtoList = new List<CategoryListDto>
        {
            new()
            {
                CategoryId = 1,
                Name = "Elektronik"
            }
        };

        _categoryServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        _mapperMock
            .Setup(x => x.Map<List<CategoryListDto>>(categories))
            .Returns(dtoList);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _categoryServiceMock.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<List<CategoryListDto>>(categories),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_ReturnsCorrectApiResponse()
    {
        var categories = new List<Category>();

        _categoryServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        _mapperMock
            .Setup(x => x.Map<List<CategoryListDto>>(categories))
            .Returns(new List<CategoryListDto>());

        var result = await _controller.GetAll();

        var ok = result as OkObjectResult;

        var response =
            ok!.Value as ApiResponse<List<CategoryListDto>>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.StatusCode.Should().Be(200);
        response.Message.Should()
            .Be("Kategoriler başarıyla listelendi.");
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        var category = new Category
        {
            CategoryId = 5,
            Name = "Kitap"
        };

        var dto = new CategoryDetailDto
        {
            CategoryId = 5,
            Name = "Kitap"
        };

        _categoryServiceMock
            .Setup(x => x.GetByIdAsync(
                5,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mapperMock
            .Setup(x => x.Map<CategoryDetailDto>(category))
            .Returns(dto);

        var result = await _controller.GetById(5);

        result.Should().BeOfType<OkObjectResult>();

        _categoryServiceMock.Verify(
            x => x.GetByIdAsync(
                5,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<CategoryDetailDto>(category),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsCorrectApiResponse()
    {
        var category = new Category
        {
            CategoryId = 3,
            Name = "Giyim"
        };

        _categoryServiceMock
            .Setup(x => x.GetByIdAsync(
                3,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mapperMock
            .Setup(x => x.Map<CategoryDetailDto>(category))
            .Returns(new CategoryDetailDto
            {
                CategoryId = 3,
                Name = "Giyim"
            });

        var result = await _controller.GetById(3);

        var ok = result as OkObjectResult;

        var response =
            ok!.Value as ApiResponse<CategoryDetailDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data!.CategoryId.Should().Be(3);
        response.Data.Name.Should().Be("Giyim");
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var dto = new CreateCategoryDto
        {
            Name = "Yeni Kategori"
        };

        var result = await _controller.Create(dto);

        result.Should().BeOfType<ObjectResult>();

        var created = result as ObjectResult;

        created!.StatusCode.Should().Be(201);

        _categoryServiceMock.Verify(
            x => x.CreateAsync(
                dto,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_ReturnsOk()
    {
        var dto = new UpdateCategoryDto
        {
            CategoryId = 2,
            Name = "Güncel"
        };

        var result = await _controller.Update(dto);

        result.Should().BeOfType<OkObjectResult>();

        _categoryServiceMock.Verify(
            x => x.UpdateAsync(
                dto,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsOk()
    {
        var result = await _controller.Delete(7);

        result.Should().BeOfType<OkObjectResult>();

        _categoryServiceMock.Verify(
            x => x.DeleteAsync(
                7,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_ReturnsCorrectApiResponse()
    {
        var dto = new CreateCategoryDto
        {
            Name = "Test"
        };

        var result = await _controller.Create(dto);

        var created = result as ObjectResult;

        var response =
            created!.Value as ApiResponse<string>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.StatusCode.Should().Be(201);
        response.Message.Should()
            .Be("Kategori başarıyla oluşturuldu.");
    }

    [Fact]
    public async Task Update_ReturnsCorrectApiResponse()
    {
        var dto = new UpdateCategoryDto();

        var result = await _controller.Update(dto);

        var ok = result as OkObjectResult;

        var response =
            ok!.Value as ApiResponse<string>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should()
            .Be("Kategori başarıyla güncellendi.");
    }

    [Fact]
    public async Task Delete_ReturnsCorrectApiResponse()
    {
        var result = await _controller.Delete(1);

        var ok = result as OkObjectResult;

        var response =
            ok!.Value as ApiResponse<string>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should()
            .Be("Kategori başarıyla silindi.");
    }
}