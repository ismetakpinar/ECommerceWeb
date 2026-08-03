using ECommerce.Web.Infrastructure.Options;
using ECommerce.Web.Services.Abstract;
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

        services.AddHttpClient<IProductService, ProductService>(
            (serviceProvider, client) =>
            {
                var settings = serviceProvider
                    .GetRequiredService<IOptions<ApiSettings>>()
                    .Value;

                client.BaseAddress = new Uri(settings.BaseUrl);
            });
        services.AddHttpClient<ICategoryService, CategoryService>(
            (serviceProvider, client) =>
    {
        var settings = serviceProvider
            .GetRequiredService<IOptions<ApiSettings>>()
            .Value;

        client.BaseAddress = new Uri(settings.BaseUrl);
    });

        return services;
    }
}