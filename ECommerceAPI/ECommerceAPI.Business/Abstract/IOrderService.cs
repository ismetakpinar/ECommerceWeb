using ECommerceAPI.Dto.Dtos.OrderDtos;
using ECommerceAPI.Entity.Entities;

namespace ECommerceAPI.Business.Abstract;

public interface IOrderService
{
    Task<List<Order>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Order> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        UpdateOrderDto dto,
        CancellationToken cancellationToken = default);

    Task<List<OrderListDto>> GetMyOrdersAsync(
        CancellationToken cancellationToken = default);

    Task<OrderDetailDto> GetMyOrderDetailAsync(
        int orderId,
        CancellationToken cancellationToken = default);

    Task CancelOrderAsync(
        int orderId,
        CancellationToken cancellationToken = default);
}