using Asp.Versioning;
using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Dto.Dtos.OrderDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerceAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[EnableRateLimiting("GeneralPolicy")]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IMapper _mapper;

    public OrdersController(
        IOrderService orderService,
        IMapper mapper)
    {
        _orderService = orderService;
        _mapper = mapper;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken = default)
    {
        var orders = await _orderService
            .GetAllAsync(cancellationToken);

        var orderDtos =
            _mapper.Map<List<OrderListDto>>(orders);

        return Ok(
            ApiResponse<List<OrderListDto>>
                .SuccessResponse(
                    orderDtos,
                    "Siparişler başarıyla listelendi."));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderService
            .GetByIdAsync(
                id,
                cancellationToken);

        var orderDto =
            _mapper.Map<OrderDetailDto>(order);

        return Ok(
            ApiResponse<OrderDetailDto>
                .SuccessResponse(
                    orderDto,
                    "Sipariş başarıyla getirildi."));
    }
    [Authorize]
    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders(
        CancellationToken cancellationToken = default)
    {
        var orders = await _orderService
            .GetMyOrdersAsync(cancellationToken);

        return Ok(
            ApiResponse<List<OrderListDto>>
                .SuccessResponse(
                    orders,
                    "Siparişler başarıyla getirildi."));
    }
    [Authorize]
    [HttpGet("my-orders/{orderId}")]
    public async Task<IActionResult> GetMyOrderDetail(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderService
            .GetMyOrderDetailAsync(
                orderId,
                cancellationToken);

        return Ok(
            ApiResponse<OrderDetailDto>
                .SuccessResponse(
                    order,
                    "Sipariş detayı başarıyla getirildi."));
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        UpdateOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        await _orderService
            .UpdateAsync(
                dto,
                cancellationToken);

        return Ok(
            ApiResponse<object>
                .SuccessResponse(
                    null,
                    "Sipariş durumu güncellendi."));
    }

    [HttpPut("{orderId}/cancel")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CancelOrder(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        await _orderService
            .CancelOrderAsync(
                orderId,
                cancellationToken);

        return Ok(
            ApiResponse<object>
                .SuccessResponse(
                    null,
                    "Sipariş iptal edildi."));
    }
}