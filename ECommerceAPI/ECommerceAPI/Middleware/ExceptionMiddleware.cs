namespace ECommerceAPI.Middleware;

using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Entity.Exceptions;
using System.Net;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beklenmeyen hata oluştu.");

            context.Response.ContentType = "application/json";

            var statusCode = HttpStatusCode.InternalServerError;

            var showDetails =
                _environment.IsDevelopment() ||
                _environment.IsEnvironment("Testing");

            var message = showDetails
                ? ex.ToString()
                : "Beklenmeyen bir hata oluştu.";

            switch (ex)
            {
                case NotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = ex.Message;
                    break;

                case BadRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = ex.Message;
                    break;

                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = ex.Message;
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<object>.ErrorResponse(
                message: message,
                statusCode: (int)statusCode,
                traceId: context.TraceIdentifier);

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}