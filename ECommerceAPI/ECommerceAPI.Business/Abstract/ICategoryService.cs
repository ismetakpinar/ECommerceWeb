using ECommerceAPI.Dto.Dtos.CategoryDtos;
using ECommerceAPI.Entity.Entities;

namespace ECommerceAPI.Business.Abstract;

public interface ICategoryService
{






    Task UpdateAsync(
    UpdateCategoryDto updateCategoryDto,
    CancellationToken cancellationToken = default);

    Task DeleteAsync(
    int id,
    CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
    int categoryId,
    CancellationToken cancellationToken = default);

    Task<List<Category>> GetAllAsync(
    CancellationToken cancellationToken = default);

    Task<Category> GetByIdAsync(
    int id,
    CancellationToken cancellationToken = default);

    Task CreateAsync(
    CreateCategoryDto createCategoryDto,
    CancellationToken cancellationToken = default);
}
