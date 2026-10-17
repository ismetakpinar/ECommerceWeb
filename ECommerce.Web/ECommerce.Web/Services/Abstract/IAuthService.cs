using ECommerce.Web.Models;
using ECommerce.Web.ViewModels.Auth;

namespace ECommerce.Web.Services.Auth;

public interface IAuthService
{
    Task<ApiResponse<string>?> RegisterAsync(
        RegisterViewModel request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<LoginResponseViewModel>?> LoginAsync(
        LoginViewModel request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<string>?> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}