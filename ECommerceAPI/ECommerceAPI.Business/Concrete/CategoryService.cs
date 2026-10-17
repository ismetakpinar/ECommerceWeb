using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.DataAccess.Specifications;
using ECommerceAPI.DataAccess.UnitOfWorks;
using ECommerceAPI.Dto.Dtos.CategoryDtos;
using ECommerceAPI.Entity.Entities;
using ECommerceAPI.Entity.Exceptions;

namespace ECommerceAPI.Business.Concrete;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    private static string NormalizeName(string name)
        => name.Trim();

    public async Task CreateAsync(
        CreateCategoryDto createCategoryDto,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Category>();

        var normalizedName =
            NormalizeName(createCategoryDto.Name);

        var categoryExists = await repository.AnyAsync(
            x => x.Name.ToLower() == normalizedName.ToLower(),
            cancellationToken);

        if (categoryExists)
        {
            throw new BadRequestException(
                "Bu isimde bir kategori zaten bulunmaktadır.");
        }

        createCategoryDto.Name = normalizedName;

        var category =
            _mapper.Map<Category>(createCategoryDto);

        await repository.AddAsync(
            category,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var categoryRepository =
            _unitOfWork.Repository<Category>();

        var productRepository =
            _unitOfWork.Repository<Product>();

        var category = await categoryRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (category is null)
        {
            throw new NotFoundException(
                "Kategori bulunamadı.");
        }

        var hasProducts = await productRepository.AnyAsync(
            x => x.CategoryId == id,
            cancellationToken);

        if (hasProducts)
        {
            throw new BadRequestException(
                "İçerisinde ürün bulunan kategori silinemez.");
        }

        categoryRepository.SoftDelete(category);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        var repository =
            _unitOfWork.Repository<Category>();

        return await repository.AnyAsync(
            x => x.CategoryId == categoryId,
            cancellationToken);
    }

    public async Task<List<Category>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var repository =
            _unitOfWork.Repository<Category>();

        return await repository.GetAllAsync(
            cancellationToken);
    }

    public async Task<Category> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var specification =
            new CategoryByIdWithProductsSpecification(id);

        var repository =
            _unitOfWork.Repository<Category>();

        var category = await repository.GetAsync(
            specification,
            cancellationToken);

        if (category is null)
        {
            throw new NotFoundException(
                "Kategori bulunamadı.");
        }

        return category;
    }

    public async Task UpdateAsync(
        UpdateCategoryDto updateCategoryDto,
        CancellationToken cancellationToken = default)
    {
        var repository =
            _unitOfWork.Repository<Category>();

        var category = await repository.GetByIdAsync(
            updateCategoryDto.CategoryId,
            cancellationToken);

        if (category is null)
        {
            throw new NotFoundException(
                "Kategori bulunamadı.");
        }

        var normalizedName =
            NormalizeName(updateCategoryDto.Name);

        var duplicateCategoryExists =
            await repository.AnyAsync(
                x =>
                    x.CategoryId != updateCategoryDto.CategoryId &&
                    x.Name.ToLower() == normalizedName.ToLower(),
                cancellationToken);

        if (duplicateCategoryExists)
        {
            throw new BadRequestException(
                "Bu isimde başka bir kategori bulunmaktadır.");
        }

        updateCategoryDto.Name = normalizedName;

        _mapper.Map(
            updateCategoryDto,
            category);

        repository.Update(category);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}