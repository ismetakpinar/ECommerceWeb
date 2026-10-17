using ECommerce.Web.Infrastructure.Authentication;
using ECommerce.Web.Infrastructure.Http;
using ECommerce.Web.Models;
using ECommerce.Web.Services.Abstract;
using ECommerce.Web.ViewModels.Address;
using ECommerce.Web.ViewModels.Checkout;

namespace ECommerce.Web.Services.Concrete;

public sealed class CheckoutService
    : BaseApiService, ICheckoutService
{
    public CheckoutService(
        HttpClient httpClient,
        ITokenService tokenService)
        : base(httpClient, tokenService)
    {
    }

    public async Task<ApiResponse<List<AddressViewModel>>?> GetAddressesAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<AddressViewModel>>(
            ApiEndpoints.Addresses.GetAll,
            cancellationToken);
    }

    public async Task<ApiResponse<object>?> CheckoutAsync(
        CheckoutViewModel model,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<CheckoutViewModel, object>(
            ApiEndpoints.Cart.Checkout,
            model,
            cancellationToken);
    }
}