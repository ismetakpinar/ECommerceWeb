using Asp.Versioning;
using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Dto.Dtos.ProductDtos;
using ECommerceAPI.Dto.Requests;
using ECommerceAPI.Entity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerceAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[EnableRateLimiting("GeneralPolicy")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;

    public ProductsController(
        IProductService productService,
        IMapper mapper)
    {
        _productService = productService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
    [FromQuery] ProductQueryRequest request,
    CancellationToken cancellationToken = default)
    {
        var pagedProducts =
            await _productService.GetPagedWithCategoryAsync(
                request,
                cancellationToken);

        var productDtos =
            _mapper.Map<List<ProductListDto>>(
                pagedProducts.Items);

        var result = new PagedResult<ProductListDto>
        {
            Items = productDtos,
            Page = pagedProducts.Page,
            PageSize = pagedProducts.PageSize,
            TotalCount = pagedProducts.TotalCount
        };

        return Ok(
            ApiResponse<PagedResult<ProductListDto>>
                .SuccessResponse(
                    result,
                    "Ürünler başarıyla listelendi."));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product =
            await _productService
                .GetByIdAsync(
                    id,
                    cancellationToken);

        var result =
            _mapper.Map<ProductDetailDto>(product);

        return Ok(
            ApiResponse<ProductDetailDto>
                .SuccessResponse(
                    result,
                    "Ürün başarıyla getirildi."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
    CreateProductDto dto,
    CancellationToken cancellationToken = default)
    {
        var product = _mapper.Map<Product>(dto);

        await _productService.CreateAsync(
            product,
            cancellationToken);

        var result = _mapper.Map<ProductDetailDto>(product);

        var apiVersion = HttpContext?.RequestServices != null
            ? HttpContext.GetRequestedApiVersion()?.ToString()
            : "1.0";

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                version = apiVersion,
                id = product.ProductId
            },
            ApiResponse<ProductDetailDto>.SuccessResponse(
                result,
                "Ürün başarıyla oluşturuldu."));
    }


    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        ProductUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        var product =
            await _productService
                .GetByIdAsync(
                    id,
                    cancellationToken);

        _mapper.Map(dto, product);

        await _productService
            .UpdateAsync(
                product,
                cancellationToken);

        return Ok(
            ApiResponse<ProductDetailDto>
                .SuccessResponse(
                    _mapper.Map<ProductDetailDto>(product),
                    "Ürün başarıyla güncellendi."));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        await _productService
            .DeleteAsync(
                id,
                cancellationToken);

        return Ok(
            ApiResponse<object>
                .SuccessResponse(
                    null,
                    "Ürün başarıyla silindi."));
    }

    [HttpPatch("{productId:int}/stock/increase")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> IncreaseStock(
        int productId,
        StockUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        await _productService
            .IncreaseStockAsync(
                productId,
                dto.Quantity,
                cancellationToken);

        return Ok(
            ApiResponse<object>
                .SuccessResponse(
                    null,
                    "Ürün stoğu başarıyla artırıldı."));
    }

    [HttpPatch("{productId:int}/stock/decrease")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DecreaseStock(
        int productId,
        StockUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        await _productService
            .DecreaseStockAsync(
                productId,
                dto.Quantity,
                cancellationToken);

        return Ok(
            ApiResponse<object>
                .SuccessResponse(
                    null,
                    "Ürün stoğu başarıyla azaltıldı."));
    }
}