using ECommerceAPI.Helpers;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace ECommerceAPI.Configurations;

public static class WebApplicationConfiguration
{
    public static WebApplication UseHealthCheckEndpoints(
        this WebApplication app)
    {
        app.MapHealthChecks(
            "/health/live",
            new HealthCheckOptions
            {
                Predicate = _ => false
            });

        app.MapHealthChecks(
            "/health/ready",
            new HealthCheckOptions
            {
                Predicate = check =>
                    check.Tags.Contains("ready"),

                ResponseWriter =
                    HealthCheckResponseWriter.WriteResponseAsync
            });

        return app;
    }
}