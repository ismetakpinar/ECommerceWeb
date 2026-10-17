using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Helpers;
using ECommerceAPI.DataAccess.Specifications;
using ECommerceAPI.DataAccess.UnitOfWorks;
using ECommerceAPI.Dto.Requests;
using ECommerceAPI.Entity.Entities;
using ECommerceAPI.Entity.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Business.Concrete;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICategoryService _categoryService;

    public ProductService(
        IUnitOfWork unitOfWork,
        ICategoryService categoryService)
    {
        _unitOfWork = unitOfWork;
        _categoryService = categoryService;
    }

    private void ValidatePagination(int page, int pageSize)
    {
        if (page < 1)
            throw new BadRequestException(
                "Sayfa numarası 1 veya daha büyük olmalıdır.");

        if (pageSize < 1 || pageSize > 100)
            throw new BadRequestException(
                "Sayfa boyutu 1 ile 100 arasında olmalıdır.");
    }

    private async Task ValidateCategoryAsync(
        int categoryId,
        CancellationToken cancellationToken)
    {
        var exists = await _categoryService.ExistsAsync(
            categoryId,
            cancellationToken);

        if (!exists)
            throw new NotFoundException("Kategori bulunamadı.");
    }

    private async Task<Product> GetProductOrThrowAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Product>();

        var product = await repository.GetByIdAsync(
            productId,
            cancellationToken);

        if (product is null)
            throw new NotFoundException("Ürün bulunamadı.");

        return product;
    }

    private async Task SaveChangesWithConcurrencyAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BadRequestException(
                "Stok bilgisi başka bir işlem tarafından değiştirildi. Lütfen tekrar deneyin.");
        }
    }

    public async Task<Product> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var specification =
            new ProductByIdWithCategorySpecification(id);

        var repository = _unitOfWork.Repository<Product>();

        var product = await repository.GetAsync(
            specification,
            cancellationToken);

        if (product is null)
            throw new NotFoundException("Ürün bulunamadı.");

        return product;
    }

    public async Task CreateAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        await ValidateCategoryAsync(
            product.CategoryId,
            cancellationToken);

        var repository = _unitOfWork.Repository<Product>();

        await repository.AddAsync(
            product,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        await ValidateCategoryAsync(
            product.CategoryId,
            cancellationToken);

        var repository = _unitOfWork.Repository<Product>();

        repository.Update(product);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Product>();

        var product = await repository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
            throw new NotFoundException("Ürün bulunamadı.");

        repository.SoftDelete(product);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<Product>> GetPagedWithCategoryAsync(
    ProductQueryRequest request,
    CancellationToken cancellationToken = default)
    {
        ValidatePagination(request.Page, request.PageSize);

        var specification =
    new ProductsWithCategorySpecification(
        request.Page,
        request.PageSize,
        request.CategoryId,
        request.IsFeatured,
        request.Search,
        request.SortBy);

        var repository = _unitOfWork.Repository<Product>();

        var totalCount = await repository.CountAsync(
            specification,
            cancellationToken);

        var products = await repository.ListAsync(
            specification,
            cancellationToken);

        return new PagedResult<Product>
        {
            Items = products,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task IncreaseStockAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Product>();

        var product = await GetProductOrThrowAsync(
            productId,
            cancellationToken);

        product.Stock += quantity;

        repository.Update(product);

        await SaveChangesWithConcurrencyAsync(cancellationToken);
    }

    public async Task DecreaseStockAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Product>();

        var product = await GetProductOrThrowAsync(
            productId,
            cancellationToken);

        if (quantity > product.Stock)
        {
            throw new BadRequestException(
                "Azaltılacak miktar mevcut stoktan fazla olamaz.");
        }

        product.Stock -= quantity;

        repository.Update(product);

        await SaveChangesWithConcurrencyAsync(cancellationToken);
    }

}