using Asp.Versioning;
using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Dto.Dtos.CustomerDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerceAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[EnableRateLimiting("GeneralPolicy")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IMapper _mapper;

    public CustomersController(
        ICustomerService customerService,
        IMapper mapper)
    {
        _customerService = customerService;
        _mapper = mapper;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken = default)
    {
        var customers = await _customerService
            .GetAllAsync(cancellationToken);

        var result =
            _mapper.Map<List<CustomerListDto>>(customers);

        return Ok(
            ApiResponse<List<CustomerListDto>>
                .SuccessResponse(
                    result,
                    "Müşteriler başarıyla listelendi."));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerService
            .GetByIdAsync(
                id,
                cancellationToken);

        var result =
            _mapper.Map<CustomerDetailDto>(customer);

        return Ok(
            ApiResponse<CustomerDetailDto>
                .SuccessResponse(
                    result,
                    "Müşteri başarıyla getirildi."));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(
        UpdateCustomerDto dto,
        CancellationToken cancellationToken = default)
    {
        await _customerService
            .UpdateAsync(
                dto,
                cancellationToken);

        return Ok(
            ApiResponse<string>
                .SuccessResponse(
                    "Müşteri başarıyla güncellendi."));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        await _customerService
            .DeleteAsync(
                id,
                cancellationToken);

        return Ok(
            ApiResponse<string>
                .SuccessResponse(
                    "Müşteri başarıyla silindi."));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerService
            .GetMeAsync(cancellationToken);

        var result =
            _mapper.Map<CustomerDetailDto>(customer);

        return Ok(
            ApiResponse<CustomerDetailDto>
                .SuccessResponse(
                    result,
                    "Müşteri bilgileri başarıyla getirildi."));
    }
}