using Asp.Versioning;
using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Dto.Dtos.CategoryDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerceAPI.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[EnableRateLimiting("GeneralPolicy")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IMapper _mapper;

    public CategoriesController(
        ICategoryService categoryService,
        IMapper mapper)
    {
        _categoryService = categoryService;
        _mapper = mapper;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();

        var result = _mapper.Map<List<CategoryListDto>>(categories);

        return Ok(
            ApiResponse<List<CategoryListDto>>.SuccessResponse(
                result,
                "Kategoriler başarıyla listelendi."));
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        var result = _mapper.Map<CategoryDetailDto>(category);

        return Ok(
            ApiResponse<CategoryDetailDto>.SuccessResponse(
                result,
                "Kategori başarıyla getirildi."));
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
    CreateCategoryDto createCategoryDto,
    CancellationToken cancellationToken = default)
    {
        await _categoryService.CreateAsync(
            createCategoryDto,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<string>.SuccessResponse(
                "Kategori başarıyla oluşturuldu.",
                StatusCodes.Status201Created));
    }
    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> Update(UpdateCategoryDto updateCategoryDto)

    {
        await _categoryService.UpdateAsync(updateCategoryDto);

        return Ok(
            ApiResponse<string>.SuccessResponse(
                "Kategori başarıyla güncellendi."));
    }
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);

        return Ok(
            ApiResponse<string>.SuccessResponse(
                "Kategori başarıyla silindi."));
    }
}