namespace ECommerce.Web.ViewModels.Auth;

public sealed class LoginResponseViewModel
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
}