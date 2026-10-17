using ECommerce.Web.Infrastructure.Authentication;
using ECommerce.Web.Infrastructure.Options;
using ECommerce.Web.Services.Abstract;
using ECommerce.Web.Services.Auth;
using ECommerce.Web.Services.Cart;
using ECommerce.Web.Services.Concrete;
using Microsoft.Extensions.Options;
namespace ECommerce.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services.Configure<ApiSettings>(
            configuration.GetSection(ApiSettings.SectionName));

        void ConfigureClient(IServiceProvider serviceProvider, HttpClient client)
        {
            var settings = serviceProvider
                .GetRequiredService<IOptions<ApiSettings>>()
                .Value;

            client.BaseAddress = new Uri(settings.BaseUrl);
        }
        services.AddHttpClient<IOrderService, OrderService>(ConfigureClient);
        services.AddHttpClient<IProductService, ProductService>(ConfigureClient);
        services.AddHttpClient<ICheckoutService, CheckoutService>(ConfigureClient);
        services.AddHttpClient<ICategoryService, CategoryService>(ConfigureClient);

        services.AddHttpClient<ICartService, CartService>(ConfigureClient);

        services.AddHttpClient<IAuthService, AuthService>(ConfigureClient);
        services.AddHttpContextAccessor();

        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}