using ECommerceAPI.Business.Helpers;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerceAPI.Configurations;

public static class RateLimiterConfiguration
{
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode =
                    StatusCodes.Status429TooManyRequests;

                var response =
                    ApiResponse<object>.ErrorResponse(
                        message:
                        "Çok fazla istek gönderdiniz. Lütfen daha sonra tekrar deneyiniz.",
                        statusCode:
                        StatusCodes.Status429TooManyRequests);

                response.TraceId =
                    context.HttpContext.TraceIdentifier;

                await context.HttpContext.Response.WriteAsJsonAsync(
                    response,
                    cancellationToken);
            };

            options.AddFixedWindowLimiter(
                "GeneralPolicy",
                limiterOptions =>
                {
                    limiterOptions.PermitLimit = 100;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueLimit = 0;
                });

            options.AddFixedWindowLimiter(
                "LoginPolicy",
                limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueLimit = 0;
                });

            options.AddFixedWindowLimiter(
                "RegisterPolicy",
                limiterOptions =>
                {
                    limiterOptions.PermitLimit = 3;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueLimit = 0;
                });

            options.AddFixedWindowLimiter(
                "RefreshPolicy",
                limiterOptions =>
                {
                    limiterOptions.PermitLimit = 10;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueLimit = 0;
                });
        });

        return services;
    }
}