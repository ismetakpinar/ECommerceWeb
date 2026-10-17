using ECommerce.Web.Infrastructure.Authentication;
using ECommerce.Web.Infrastructure.Http;
using ECommerce.Web.Models;
using ECommerce.Web.ViewModels.Auth;

namespace ECommerce.Web.Services.Auth;

public sealed class AuthService : BaseApiService, IAuthService
{
    private sealed record RegisterRequest(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string Phone);

    private sealed record RefreshTokenRequest(string RefreshToken);

    public AuthService(
    HttpClient httpClient,
    ITokenService tokenService)
    : base(httpClient, tokenService)
    {
    }

    public async Task<ApiResponse<string>?> RegisterAsync(
        RegisterViewModel request,
        CancellationToken cancellationToken = default)
    {
        var payload = new RegisterRequest(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.Phone);

        return await PostAsync<RegisterRequest, string>(
            ApiEndpoints.Auth.Register,
            payload,
            cancellationToken);
    }

    public async Task<ApiResponse<LoginResponseViewModel>?> LoginAsync(
        LoginViewModel request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<LoginViewModel, LoginResponseViewModel>(
            ApiEndpoints.Auth.Login,
            request,
            cancellationToken);
    }

    public async Task<ApiResponse<string>?> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<RefreshTokenRequest, string>(
            ApiEndpoints.Auth.Logout,
            new RefreshTokenRequest(refreshToken),
            cancellationToken);
    }
}