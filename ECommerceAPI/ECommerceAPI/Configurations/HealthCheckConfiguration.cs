using ECommerceAPI.DataAccess.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ECommerceAPI.Configurations;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddHealthChecksConfiguration(
        this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddDbContextCheck<ECommerceDbContext>(
                name: "sqlserver",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "database", "ready" });

        return services;
    }
}