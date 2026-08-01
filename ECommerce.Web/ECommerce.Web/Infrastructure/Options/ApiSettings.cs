namespace ECommerce.Web.Infrastructure.Options;

public sealed class ApiSettings
{
    public const string SectionName = "ApiSettings";

    public string BaseUrl { get; init; } = string.Empty;
}