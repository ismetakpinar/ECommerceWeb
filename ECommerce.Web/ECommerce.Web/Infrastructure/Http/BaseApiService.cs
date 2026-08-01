using ECommerce.Web.Models;
using System.Text.Json;

namespace ECommerce.Web.Infrastructure.Http;

public abstract class BaseApiService
{
    protected readonly HttpClient HttpClient;
    protected readonly JsonSerializerOptions JsonOptions;

    protected BaseApiService(HttpClient httpClient)
    {
        HttpClient = httpClient;

        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    protected async Task<ApiResponse<TResponse>?> GetAsync<TResponse>(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        var response = await HttpClient.GetAsync(
            endpoint,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(
            JsonOptions,
            cancellationToken);
    }

    protected async Task<ApiResponse<TResponse>?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await HttpClient.PostAsJsonAsync(
            endpoint,
            request,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(
            JsonOptions,
            cancellationToken);
    }

    protected async Task<ApiResponse<TResponse>?> PutAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await HttpClient.PutAsJsonAsync(
            endpoint,
            request,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(
            JsonOptions,
            cancellationToken);
    }

    protected async Task<ApiResponse<object>?> DeleteAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        var response = await HttpClient.DeleteAsync(
            endpoint,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<ApiResponse<object>>(
            JsonOptions,
            cancellationToken);
    }
}