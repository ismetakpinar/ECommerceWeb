using ECommerce.Web.Infrastructure.Authentication;
using ECommerce.Web.Models;
using ECommerce.Web.ViewModels.Auth;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ECommerce.Web.Infrastructure.Http;

public abstract class BaseApiService
{
    protected readonly HttpClient HttpClient;
    protected readonly JsonSerializerOptions JsonOptions;
    protected readonly ITokenService TokenService;

    protected BaseApiService(
        HttpClient httpClient,
        ITokenService tokenService)
    {
        HttpClient = httpClient;
        TokenService = tokenService;

        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    protected async Task<ApiResponse<TResponse>?> GetAsync<TResponse>(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        return await SendAuthorizedAsync<TResponse>(
            () => HttpClient.GetAsync(endpoint, cancellationToken),
            cancellationToken);
    }

    protected async Task<ApiResponse<TResponse>?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        return await SendAuthorizedAsync<TResponse>(
            () => HttpClient.PostAsJsonAsync(endpoint, request, cancellationToken),
            cancellationToken);
    }

    protected async Task<ApiResponse<TResponse>?> PutAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        return await SendAuthorizedAsync<TResponse>(
            () => HttpClient.PutAsJsonAsync(endpoint, request, cancellationToken),
            cancellationToken);
    }

    protected async Task<ApiResponse<object>?> DeleteAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        return await SendAuthorizedAsync<object>(
            () => HttpClient.DeleteAsync(endpoint, cancellationToken),
            cancellationToken);
    }

    private async Task<ApiResponse<T>?> SendAuthorizedAsync<T>(
        Func<Task<HttpResponseMessage>> sendRequest,
        CancellationToken cancellationToken)
    {
        AddAuthorizationHeader();
        var response = await sendRequest();

        if (response.StatusCode == HttpStatusCode.Unauthorized &&
            !string.IsNullOrWhiteSpace(TokenService.GetRefreshToken()))
        {
            response.Dispose();

            if (!await TryRefreshTokensAsync(cancellationToken))
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Oturum süreniz doldu. Lütfen yeniden giriş yapın."
                };
            }

            AddAuthorizationHeader();
            response = await sendRequest();
        }

        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    private async Task<bool> TryRefreshTokensAsync(
        CancellationToken cancellationToken)
    {
        var refreshToken = TokenService.GetRefreshToken();
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            TokenService.RemoveTokens();
            return false;
        }

        using var response = await HttpClient.PostAsJsonAsync(
            ApiEndpoints.Auth.RefreshToken,
            new RefreshTokenRequest(refreshToken),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            TokenService.RemoveTokens();
            return false;
        }

        var result = await response.Content
            .ReadFromJsonAsync<ApiResponse<LoginResponseViewModel>>(
                JsonOptions,
                cancellationToken);

        if (result?.Success != true || result.Data is null)
        {
            TokenService.RemoveTokens();
            return false;
        }

        await TokenService.SetTokensAsync(
            result.Data.AccessToken,
            result.Data.RefreshToken);

        return true;
    }

    private sealed record RefreshTokenRequest(string RefreshToken);

    private async Task<ApiResponse<T>?> ReadResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            ApiResponse<object>? errorResponse = null;

            try
            {
                errorResponse =
                    await response.Content.ReadFromJsonAsync<ApiResponse<object>>(
                        JsonOptions,
                        cancellationToken);
            }
            catch
            {
                // API standart format dışında cevap döndürmüş olabilir.
            }

            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = errorResponse?.Message
                          ?? $"HTTP {(int)response.StatusCode} hatası oluştu."
            };
        }

        if (response.Content.Headers.ContentLength == 0)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = (int)response.StatusCode
            };
        }

        return await response.Content.ReadFromJsonAsync<ApiResponse<T>>(
            JsonOptions,
            cancellationToken);
    }

    protected void AddAuthorizationHeader()
    {
        var token = TokenService.GetAccessToken();

        if (string.IsNullOrWhiteSpace(token))
        {
            HttpClient.DefaultRequestHeaders.Authorization = null;
            return;
        }

        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}