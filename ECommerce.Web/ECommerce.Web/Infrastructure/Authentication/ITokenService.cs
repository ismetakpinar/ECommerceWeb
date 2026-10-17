namespace ECommerce.Web.Infrastructure.Authentication;

public interface ITokenService
{
    Task SetTokensAsync(
        string accessToken,
        string refreshToken);

    string? GetAccessToken();

    string? GetRefreshToken();

    void RemoveTokens();
}