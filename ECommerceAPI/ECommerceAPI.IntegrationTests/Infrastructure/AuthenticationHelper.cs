using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Dto.Dtos;
using ECommerceAPI.Dto.Dtos.AuthDtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceAPI.IntegrationTests.Infrastructure;

public static class AuthenticationHelper
{
    private static string? _adminAccessToken;

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    private const string LoginEndpoint = "/api/v1.0/Auth/login";

    public static async Task AuthenticateAsAdminAsync(HttpClient client)
    {
        if (string.IsNullOrWhiteSpace(_adminAccessToken))
        {
            _adminAccessToken = await LoginAsync(
                client,
                TestConstants.AdminEmail,
                TestConstants.AdminPassword);
        }

        SetBearerToken(client, _adminAccessToken);
    }

    public static async Task AuthenticateAsUserAsync(HttpClient client)
    {
        // Implementation for authenticating as a regular user
    }

    public static void ClearAuthentication(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }

    public static void SetBearerToken(
        HttpClient client,
        string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);
    }

    private static async Task<string> LoginAsync(
        HttpClient client,
        string email,
        string password)
    {
        var response =
            await client.PostAsJsonAsync(
                LoginEndpoint,
                new LoginDto
                {
                    Email = email,
                    Password = password
                });

        response.EnsureSuccessStatusCode();

        var apiResponse =
            await response.Content.ReadFromJsonAsync<
                ApiResponse<LoginResponseDto>>(JsonOptions);

        if (apiResponse?.Data is null)
        {
            throw new InvalidOperationException(
                "Login response is invalid.");
        }

        return apiResponse.Data.AccessToken;
    }
}