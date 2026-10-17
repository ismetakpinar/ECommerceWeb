using ECommerceAPI.DataAccess.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ECommerceAPI.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ECommerceDbContext>>();

            services.AddDbContext<ECommerceDbContext>(options =>
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.Testing.json")
                    .Build();

                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"));
            });
        });
    }
}