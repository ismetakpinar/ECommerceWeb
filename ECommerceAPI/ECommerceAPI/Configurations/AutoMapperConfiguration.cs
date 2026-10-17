using ECommerceAPI.Business.Mapping;

namespace ECommerceAPI.Configurations;

public static class AutoMapperConfiguration
{
    public static IServiceCollection AddAutoMapperConfiguration(
        this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<GeneralMapping>();
        });

        return services;
    }
}