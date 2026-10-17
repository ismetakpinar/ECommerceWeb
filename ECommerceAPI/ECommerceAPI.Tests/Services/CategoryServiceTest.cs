using AutoMapper;
using ECommerceAPI.Business.Concrete;
using ECommerceAPI.DataAccess.Specifications;
using ECommerceAPI.DataAccess.UnitOfWorks;
using ECommerceAPI.Dto.Dtos.CategoryDtos;
using ECommerceAPI.Entity.Entities;
using ECommerceAPI.Entity.Exceptions;
using FluentAssertions;
using Moq;

namespace ECommerceAPI.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly Mock<IGenericRepository<Category>>
        _categoryRepositoryMock;

    private readonly Mock<IGenericRepository<Product>>
        _productRepositoryMock;

    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

        _categoryRepositoryMock =
            new Mock<IGenericRepository<Category>>();

        _productRepositoryMock =
            new Mock<IGenericRepository<Product>>();

        _unitOfWorkMock
            .Setup(x => x.Repository<Category>())
            .Returns(_categoryRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.Repository<Product>())
            .Returns(_productRepositoryMock.Object);

        _categoryService = new CategoryService(
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_CategoriesExist_ReturnsCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new()
            {
                CategoryId = 1,
                Name = "Elektronik"
            },
            new()
            {
                CategoryId = 2,
                Name = "Bilgisayar"
            }
        };

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _categoryService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(categories);
        result.Should().HaveCount(2);

        _categoryRepositoryMock.Verify(
            x => x.GetAllAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_CategoryExists_ReturnsCategory()
    {
        // Arrange
        var category = new Category
        {
            CategoryId = 1,
            Name = "Elektronik"
        };

        _categoryRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Category>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _categoryService.GetByIdAsync(
            category.CategoryId);

        // Assert
        result.Should().NotBeNull();
        result.CategoryId.Should().Be(category.CategoryId);
        result.Name.Should().Be(category.Name);

        _categoryRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<ISpecification<Category>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_CategoryDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int categoryId = 999;

        _categoryRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Category>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        Func<Task> act = async () =>
            await _categoryService.GetByIdAsync(categoryId);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Kategori bulunamadı.");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistsAsync_ReturnsRepositoryResult(
        bool categoryExists)
    {
        // Arrange
        const int categoryId = 1;

        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(
                It.IsAny<System.Linq.Expressions.Expression<
                    Func<Category, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryExists);

        // Act
        var result = await _categoryService.ExistsAsync(categoryId);

        // Assert
        result.Should().Be(categoryExists);

        _categoryRepositoryMock.Verify(
            x => x.AnyAsync(
                It.IsAny<System.Linq.Expressions.Expression<
                    Func<Category, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CategoryNameIsUnique_AddsCategoryAndSavesChanges()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            Name = "  Elektronik  "
        };

        var mappedCategory = new Category
        {
            CategoryId = 1,
            Name = "Elektronik"
        };

        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(
                It.IsAny<System.Linq.Expressions.Expression<
                    Func<Category, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mapperMock
            .Setup(x => x.Map<Category>(createCategoryDto))
            .Returns(mappedCategory);

        // Act
        await _categoryService.CreateAsync(createCategoryDto);

        // Assert
        createCategoryDto.Name.Should().Be("Elektronik");

        _mapperMock.Verify(
            x => x.Map<Category>(createCategoryDto),
            Times.Once);

        _categoryRepositoryMock.Verify(
            x => x.AddAsync(
                mappedCategory,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CategoryNameAlreadyExists_ThrowsBadRequestException()
    {
        // Arrange
        var createCategoryDto = new CreateCategoryDto
        {
            Name = "Elektronik"
        };

        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(
                It.IsAny<System.Linq.Expressions.Expression<
                    Func<Category, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () =>
            await _categoryService.CreateAsync(
                createCategoryDto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "Bu isimde bir kategori zaten bulunmaktadır.");

        _mapperMock.Verify(
            x => x.Map<Category>(
                It.IsAny<CreateCategoryDto>()),
            Times.Never);

        _categoryRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Category>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_CategoryExistsAndNameIsUnique_UpdatesCategoryAndSavesChanges()
    {
        // Arrange
        var category = new Category
        {
            CategoryId = 1,
            Name = "Eski kategori adı"
        };

        var updateCategoryDto = new UpdateCategoryDto
        {
            CategoryId = category.CategoryId,
            Name = "  Elektronik  "
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(
                category.CategoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(
                It.IsAny<System.Linq.Expressions.Expression<
                    Func<Category, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mapperMock
            .Setup(x => x.Map(
                updateCategoryDto,
                category))
            .Callback(() =>
            {
                category.Name = updateCategoryDto.Name;
            });

        // Act
        await _categoryService.UpdateAsync(
            updateCategoryDto);

        // Assert
        updateCategoryDto.Name.Should().Be("Elektronik");

        _mapperMock.Verify(
            x => x.Map(
                updateCategoryDto,
                category),
            Times.Once);

        _categoryRepositoryMock.Verify(
            x => x.Update(category),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CategoryDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var updateCategoryDto = new UpdateCategoryDto
        {
            CategoryId = 999,
            Name = "Elektronik"
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(
                updateCategoryDto.CategoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        Func<Task> act = async () =>
            await _categoryService.UpdateAsync(
                updateCategoryDto);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Kategori bulunamadı.");

        _categoryRepositoryMock.Verify(
            x => x.AnyAsync(
                It.IsAny<System.Linq.Expressions.Expression<
                    Func<Category, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mapperMock.Verify(
            x => x.Map(
                It.IsAny<UpdateCategoryDto>(),
                It.IsAny<Category>()),
            Times.Never);

        _categoryRepositoryMock.Verify(
            x => x.Update(It.IsAny<Category>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_CategoryNameAlreadyExists_ThrowsBadRequestException()
    {
        // Arrange
        var category = new Category
        {
            CategoryId = 1,
            Name = "Eski kategori adı"
        };

        var updateCategoryDto = new UpdateCategoryDto
        {
            CategoryId = category.CategoryId,
            Name = "Elektronik"
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(
                category.CategoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(
                It.IsAny<System.Linq.Expressions.Expression<
                    Func<Category, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () =>
            await _categoryService.UpdateAsync(
                updateCategoryDto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "Bu isimde başka bir kategori bulunmaktadır.");

        _mapperMock.Verify(
            x => x.Map(
                It.IsAny<UpdateCategoryDto>(),
                It.IsAny<Category>()),
            Times.Never);

        _categoryRepositoryMock.Verify(
            x => x.Update(It.IsAny<Category>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_CategoryExistsAndHasNoProducts_SoftDeletesCategoryAndSavesChanges()
    {
        // Arrange
        var category = new Category
        {
            CategoryId = 1,
            Name = "Elektronik"
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(
                category.CategoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepositoryMock
            .Setup(x => x.AnyAsync(
                It.IsAny<System.Linq.Expressions.Expression<
                    Func<Product, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _categoryService.DeleteAsync(
            category.CategoryId);

        // Assert
        _categoryRepositoryMock.Verify(
            x => x.SoftDelete(category),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CategoryDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int categoryId = 999;

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(
                categoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        Func<Task> act = async () =>
            await _categoryService.DeleteAsync(categoryId);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Kategori bulunamadı.");

        _productRepositoryMock.Verify(
            x => x.AnyAsync(
                It.IsAny<System.Linq.Expressions.Expression<
                    Func<Product, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _categoryRepositoryMock.Verify(
            x => x.SoftDelete(It.IsAny<Category>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_CategoryHasProducts_ThrowsBadRequestException()
    {
        // Arrange
        var category = new Category
        {
            CategoryId = 1,
            Name = "Elektronik"
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(
                category.CategoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepositoryMock
            .Setup(x => x.AnyAsync(
                It.IsAny<System.Linq.Expressions.Expression<
                    Func<Product, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () =>
            await _categoryService.DeleteAsync(
                category.CategoryId);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "İçerisinde ürün bulunan kategori silinemez.");

        _categoryRepositoryMock.Verify(
            x => x.SoftDelete(It.IsAny<Category>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}