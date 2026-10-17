using ECommerce.Web.Infrastructure.Authentication;
using ECommerce.Web.Infrastructure.Http;
using ECommerce.Web.Models;
using ECommerce.Web.ViewModels.Order;

public class OrderService
    : BaseApiService, IOrderService
{
    public OrderService(
        HttpClient httpClient,
        ITokenService tokenService)
        : base(httpClient, tokenService)
    {
    }

    public async Task<ApiResponse<List<OrderListViewModel>>?> GetMyOrdersAsync(
    CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<OrderListViewModel>>(
            ApiEndpoints.Orders.MyOrders,
            cancellationToken);
    }

    public async Task<ApiResponse<OrderDetailViewModel>?> GetMyOrderDetailAsync(
    int orderId,
    CancellationToken cancellationToken = default)
    {
        return await GetAsync<OrderDetailViewModel>(
            $"{ApiEndpoints.Orders.MyOrders}/{orderId}",
            cancellationToken);
    }
}