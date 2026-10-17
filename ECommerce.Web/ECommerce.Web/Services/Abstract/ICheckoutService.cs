using ECommerce.Web.Models;
using ECommerce.Web.ViewModels.Address;
using ECommerce.Web.ViewModels.Checkout;

namespace ECommerce.Web.Services.Abstract;

public interface ICheckoutService
{
    Task<ApiResponse<List<AddressViewModel>>?> GetAddressesAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object>?> CheckoutAsync(
        CheckoutViewModel model,
        CancellationToken cancellationToken = default);
}