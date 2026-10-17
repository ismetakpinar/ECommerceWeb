using ECommerceAPI.Business.Helpers;
using ECommerceAPI.DataAccess.Context;
using ECommerceAPI.DataAccess.Initializers;
using ECommerceAPI.Dto.Dtos.AddressDtos;
using ECommerceAPI.Dto.Dtos.CartDtos;
using ECommerceAPI.Dto.Dtos.ProductDtos;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceAPI.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected HttpClient Client = default!;
    protected readonly CustomWebApplicationFactory Factory;

    protected const string ApiVersion = "/api/v1.0";

    protected static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
    }

    protected async Task AuthenticateAsAdminAsync()
    {
        await AuthenticationHelper.AuthenticateAsAdminAsync(Client);
    }

    protected void ClearAuthentication()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    protected static async Task<ApiResponse<T>> ReadResponseAsync<T>(
        HttpResponseMessage response)
    {
        var result =
            await response.Content.ReadFromJsonAsync<ApiResponse<T>>(
                JsonOptions);

        result.Should().NotBeNull();

        return result!;
    }

    protected async Task<int> CreateProductAsync(
        CreateProductDto? dto = null)
    {
        dto ??= TestDataBuilder.CreateProduct();

        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Products",
            dto);

        response.EnsureSuccessStatusCode();

        var result =
            await ReadResponseAsync<ProductDetailDto>(response);

        result.Data.Should().NotBeNull();

        var product = result.Data!;

        return product.ProductId;
    }

    protected async Task<int> CreateAddressAsync(
        CreateAddressDto? dto = null)
    {
        dto ??= new CreateAddressDto
        {
            Title = "Ev",
            City = "İstanbul",
            District = "Kadıköy",
            FullAddress = "Test Mahallesi No:1"
        };

        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Addresses",
            dto);

        response.EnsureSuccessStatusCode();

        using var scope = Factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ECommerceDbContext>();

        return await db.Addresses
            .AsNoTracking()
            .Where(x =>
                x.Title == dto.Title &&
                x.City == dto.City &&
                x.District == dto.District &&
                x.FullAddress == dto.FullAddress &&
                !x.IsDeleted)
            .Select(x => x.AddressId)
            .SingleAsync();
    }

    public virtual async Task InitializeAsync()
    {
        using var scope = Factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ECommerceDbContext>();

        await db.Database.EnsureDeletedAsync();

        await DatabaseInitializer.InitializeAsync(db);

        Client = Factory.CreateClient();
    }

    public virtual Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    protected async Task AddProductToCartAsync(
        int productId,
        int quantity = 1)
    {
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Cart/add",
            new AddToCartDto
            {
                ProductId = productId,
                Quantity = quantity
            });

        response.EnsureSuccessStatusCode();
    }
}