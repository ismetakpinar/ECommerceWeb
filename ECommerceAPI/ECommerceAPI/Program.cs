using ECommerceAPI.Configurations;
using ECommerceAPI.DataAccess.Initializers;
using ECommerceAPI.Middleware;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/log-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddDependencyInjection();

builder.Services.AddCompressionConfiguration();

builder.Services.AddMemoryCache();

builder.Services.AddAutoMapperConfiguration();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddApiVersioningConfiguration();

builder.Services.AddSwaggerDocumentation();

builder.Services.AddValidationConfiguration();

builder.Services.AddRateLimiting();

builder.Services.AddHealthChecksConfiguration();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await DatabaseInitializer.InitializeAsync(app.Services);

    app.UseSwagger();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("ECommerce API");
        options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
    });
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseResponseCompression();

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.UseHealthCheckEndpoints();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program
{
}