using ECommerce.Web.Models;
using ECommerce.Web.ViewModels.Order;

public interface IOrderService
{
    Task<ApiResponse<List<OrderListViewModel>>?> GetMyOrdersAsync(
    CancellationToken cancellationToken = default);

    Task<ApiResponse<OrderDetailViewModel>?> GetMyOrderDetailAsync(
        int orderId,
        CancellationToken cancellationToken = default);
}