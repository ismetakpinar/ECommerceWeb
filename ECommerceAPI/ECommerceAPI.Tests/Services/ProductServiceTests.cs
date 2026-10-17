using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Concrete;
using ECommerceAPI.DataAccess.Specifications;
using ECommerceAPI.DataAccess.UnitOfWorks;
using ECommerceAPI.Dto.Requests;
using ECommerceAPI.Entity.Entities;
using ECommerceAPI.Entity.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ECommerceAPI.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoryService> _categoryServiceMock;
    private readonly ProductService _productService;
    private readonly Mock<IGenericRepository<Product>> _productRepositoryMock;
    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryServiceMock = new Mock<ICategoryService>();
        _productRepositoryMock = new Mock<IGenericRepository<Product>>();

        _unitOfWorkMock
            .Setup(x => x.Repository<Product>())
            .Returns(_productRepositoryMock.Object);

        _productService = new ProductService(
            _unitOfWorkMock.Object,
            _categoryServiceMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ProductExists_ReturnsProduct()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 1,
            Name = "Laptop"
        };

        _productRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Product>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);


        var result = await _productService.GetByIdAsync(1);


        result.Should().NotBeNull();
        result.ProductId.Should().Be(1);
        result.Name.Should().Be("Laptop");
    }
    [Fact]
    public async Task GetByIdAsync_ProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _productRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Product>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () =>
            await _productService.GetByIdAsync(999);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Ürün bulunamadı.");
    }
    [Fact]
    public async Task CreateAsync_CategoryExists_AddsProductAndSavesChanges()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 1,
            Name = "Laptop",
            CategoryId = 1
        };

        _categoryServiceMock
            .Setup(x => x.ExistsAsync(
                product.CategoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _productService.CreateAsync(product);

        // Assert
        _productRepositoryMock.Verify(
            x => x.AddAsync(
                product,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task CreateAsync_CategoryDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 1,
            Name = "Laptop",
            CategoryId = 99
        };

        _categoryServiceMock
            .Setup(x => x.ExistsAsync(
                product.CategoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () =>
            await _productService.CreateAsync(product);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Kategori bulunamadı.");

        _productRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task UpdateAsync_CategoryExists_UpdatesProductAndSavesChanges()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 1,
            Name = "Güncellenmiş Laptop",
            CategoryId = 1
        };

        _categoryServiceMock
            .Setup(x => x.ExistsAsync(
                product.CategoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _productService.UpdateAsync(product);

        // Assert
        _categoryServiceMock.Verify(
            x => x.ExistsAsync(
                product.CategoryId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _productRepositoryMock.Verify(
            x => x.Update(product),
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
        var product = new Product
        {
            ProductId = 1,
            Name = "Laptop",
            CategoryId = 999
        };

        _categoryServiceMock
            .Setup(x => x.ExistsAsync(
                product.CategoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () =>
            await _productService.UpdateAsync(product);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Kategori bulunamadı.");

        _productRepositoryMock.Verify(
            x => x.Update(It.IsAny<Product>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task DeleteAsync_ProductExists_SoftDeletesProductAndSavesChanges()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 1,
            Name = "Laptop"
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                product.ProductId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _productService.DeleteAsync(product.ProductId);

        // Assert
        _productRepositoryMock.Verify(
            x => x.GetByIdAsync(
                product.ProductId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _productRepositoryMock.Verify(
            x => x.SoftDelete(product),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task DeleteAsync_ProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () =>
            await _productService.DeleteAsync(999);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Ürün bulunamadı.");

        _productRepositoryMock.Verify(
            x => x.SoftDelete(It.IsAny<Product>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task GetPagedWithCategoryAsync_ValidPagination_ReturnsPagedResult()
    {
        // Arrange
        const int page = 1;
        const int pageSize = 2;
        const int totalCount = 5;

        var products = new List<Product>
    {
        new()
        {
            ProductId = 1,
            Name = "Laptop",
            CategoryId = 1
        },
        new()
        {
            ProductId = 2,
            Name = "Telefon",
            CategoryId = 1
        }
    };

        _productRepositoryMock
            .Setup(x => x.CountAsync(
                It.IsAny<ISpecification<Product>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);

        _productRepositoryMock
            .Setup(x => x.ListAsync(
                It.IsAny<ISpecification<Product>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var request = new ProductQueryRequest
        {
            Page = page,
            PageSize = pageSize
        };

        var result = await _productService
            .GetPagedWithCategoryAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEquivalentTo(products);
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.TotalCount.Should().Be(totalCount);

        _productRepositoryMock.Verify(
            x => x.CountAsync(
                It.IsAny<ISpecification<Product>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _productRepositoryMock.Verify(
            x => x.ListAsync(
                It.IsAny<ISpecification<Product>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task GetPagedWithCategoryAsync_PageLessThanOne_ThrowsBadRequestException()
    {
        // Arrange
        const int page = 0;
        const int pageSize = 10;

        // Act
        Func<Task> act = async () =>
            await _productService.GetPagedWithCategoryAsync(
    new ProductQueryRequest
    {
        Page = page,
        PageSize = pageSize
    });

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Sayfa numarası 1 veya daha büyük olmalıdır.");

        _productRepositoryMock.Verify(
            x => x.CountAsync(
                It.IsAny<ISpecification<Product>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _productRepositoryMock.Verify(
            x => x.ListAsync(
                It.IsAny<ISpecification<Product>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetPagedWithCategoryAsync_InvalidPageSize_ThrowsBadRequestException(int pageSize)
    {
        // Arrange
        const int page = 1;

        // Act
        Func<Task> act = async () =>
            await _productService.GetPagedWithCategoryAsync(
    new ProductQueryRequest
    {
        Page = page,
        PageSize = pageSize
    });

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır.");

        _productRepositoryMock.Verify(
            x => x.CountAsync(
                It.IsAny<ISpecification<Product>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _productRepositoryMock.Verify(
            x => x.ListAsync(
                It.IsAny<ISpecification<Product>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task IncreaseStockAsync_ProductExists_IncreasesStockAndSavesChanges()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 1,
            Name = "Laptop",
            Stock = 10
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                product.ProductId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _productService.IncreaseStockAsync(
            product.ProductId,
            5);

        // Assert
        product.Stock.Should().Be(15);

        _productRepositoryMock.Verify(
            x => x.Update(product),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task IncreaseStockAsync_SaveChangesThrowsConcurrencyException_ThrowsBadRequestException()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 1,
            Name = "Laptop",
            Stock = 10
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                product.ProductId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        // Act
        Func<Task> act = async () =>
            await _productService.IncreaseStockAsync(
                product.ProductId,
                5);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Stok bilgisi başka bir işlem tarafından değiştirildi. Lütfen tekrar deneyin.");

        _productRepositoryMock.Verify(
            x => x.Update(product),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task DecreaseStockAsync_ProductExists_DecreasesStockAndSavesChanges()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 1,
            Name = "Laptop",
            Stock = 20
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                product.ProductId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _productService.DecreaseStockAsync(
            product.ProductId,
            5);

        // Assert
        product.Stock.Should().Be(15);

        _productRepositoryMock.Verify(
            x => x.Update(product),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task IncreaseStockAsync_ProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int productId = 999;
        const int quantity = 5;

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                productId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () =>
            await _productService.IncreaseStockAsync(
                productId,
                quantity);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Ürün bulunamadı.");

        _productRepositoryMock.Verify(
            x => x.Update(It.IsAny<Product>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task DecreaseStockAsync_QuantityGreaterThanStock_ThrowsBadRequestException()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 1,
            Name = "Laptop",
            Stock = 10
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                product.ProductId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        Func<Task> act = async () =>
            await _productService.DecreaseStockAsync(
                product.ProductId,
                15);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Azaltılacak miktar mevcut stoktan fazla olamaz.");

        product.Stock.Should().Be(10);

        _productRepositoryMock.Verify(
            x => x.Update(It.IsAny<Product>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task DecreaseStockAsync_ProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int productId = 999;
        const int quantity = 5;

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                productId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () =>
            await _productService.DecreaseStockAsync(
                productId,
                quantity);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Ürün bulunamadı.");

        _productRepositoryMock.Verify(
            x => x.Update(It.IsAny<Product>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task DecreaseStockAsync_SaveChangesThrowsConcurrencyException_ThrowsBadRequestException()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 1,
            Name = "Laptop",
            Stock = 20
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                product.ProductId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        // Act
        Func<Task> act = async () =>
            await _productService.DecreaseStockAsync(
                product.ProductId,
                5);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Stok bilgisi başka bir işlem tarafından değiştirildi. Lütfen tekrar deneyin.");

        _productRepositoryMock.Verify(
            x => x.Update(product),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
