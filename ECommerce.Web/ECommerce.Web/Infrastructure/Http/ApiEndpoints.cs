namespace ECommerce.Web.Infrastructure.Http;

public static class ApiEndpoints
{
    private const string Api = "api/v1";

    public const string Auth = $"{Api}/Auth";

    public const string Products = $"{Api}/Products";

    public const string Categories = $"{Api}/Categories";

    public const string Customers = $"{Api}/Customers";

    public const string Addresses = $"{Api}/Addresses";

    public const string Orders = $"{Api}/Orders";

    public const string Cart = $"{Api}/Cart";
}