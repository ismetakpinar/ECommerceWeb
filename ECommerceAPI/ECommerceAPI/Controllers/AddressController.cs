using Asp.Versioning;
using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Dto.Dtos.AddressDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerceAPI.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[EnableRateLimiting("GeneralPolicy")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AddressesController : ControllerBase
{
    private readonly IAddressService _addressService;
    private readonly IMapper _mapper;

    public AddressesController(
        IAddressService addressService,
        IMapper mapper)
    {
        _addressService = addressService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken = default)
    {
        var addresses = await _addressService
            .GetAllAsync(cancellationToken);

        var result =
            _mapper.Map<List<AddressListDto>>(addresses);

        return Ok(
            ApiResponse<List<AddressListDto>>
                .SuccessResponse(
                    result,
                    "Adresler başarıyla listelendi."));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var address = await _addressService
            .GetByIdAsync(
                id,
                cancellationToken);

        var result =
            _mapper.Map<AddressDetailDto>(address);

        return Ok(
            ApiResponse<AddressDetailDto>
                .SuccessResponse(
                    result,
                    "Adres başarıyla getirildi."));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateAddressDto dto,
        CancellationToken cancellationToken = default)
    {
        await _addressService
            .CreateAsync(
                dto,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<object>
                .SuccessResponse(
                    null,
                    "Adres başarıyla oluşturuldu.",
                    StatusCodes.Status201Created));
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        UpdateAddressDto dto,
        CancellationToken cancellationToken = default)
    {
        await _addressService
            .UpdateAsync(
                dto,
                cancellationToken);

        return Ok(
            ApiResponse<object>
                .SuccessResponse(
                    null,
                    "Adres başarıyla güncellendi."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        await _addressService
            .DeleteAsync(
                id,
                cancellationToken);

        return Ok(
            ApiResponse<object>
                .SuccessResponse(
                    null,
                    "Adres başarıyla silindi."));
    }
}