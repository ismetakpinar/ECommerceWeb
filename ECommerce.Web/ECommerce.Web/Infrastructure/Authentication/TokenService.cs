namespace ECommerce.Web.Infrastructure.Authentication;

public sealed class TokenService : ITokenService
{
    private const string AccessTokenKey = "AccessToken";
    private const string RefreshTokenKey = "RefreshToken";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task SetTokensAsync(
        string accessToken,
        string refreshToken)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };

        _httpContextAccessor.HttpContext!
            .Response.Cookies.Append(
                AccessTokenKey,
                accessToken,
                options);

        _httpContextAccessor.HttpContext!
            .Response.Cookies.Append(
                RefreshTokenKey,
                refreshToken,
                options);

        return Task.CompletedTask;
    }

    public string? GetAccessToken()
    {
        _httpContextAccessor.HttpContext!
            .Request.Cookies.TryGetValue(
                AccessTokenKey,
                out var token);

        return token;
    }

    public string? GetRefreshToken()
    {
        _httpContextAccessor.HttpContext!
            .Request.Cookies.TryGetValue(
                RefreshTokenKey,
                out var token);

        return token;
    }

    public void RemoveTokens()
    {
        _httpContextAccessor.HttpContext!
            .Response.Cookies.Delete(AccessTokenKey);

        _httpContextAccessor.HttpContext!
            .Response.Cookies.Delete(RefreshTokenKey);
    }
}
