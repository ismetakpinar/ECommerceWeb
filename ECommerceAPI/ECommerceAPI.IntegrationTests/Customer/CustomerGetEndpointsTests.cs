using ECommerceAPI.Dto.Dtos.CustomerDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;

namespace ECommerceAPI.IntegrationTests.Customer;

public class CustomerGetEndpointsTests
    : IntegrationTestBase
{
    public CustomerGetEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetAll_Should_Return_Ok_When_User_Is_Admin()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<List<CustomerListDto>>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Müşteriler başarıyla listelendi.");

        result.Data.Should().NotBeNull();
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAll_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_Should_Return_Ok_When_Customer_Exists()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        const int customerId = 1;

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<CustomerDetailDto>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Müşteri başarıyla getirildi.");

        result.Data.Should().NotBeNull();

        var customer = result.Data!;

        customer.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Customer_Does_Not_Exist()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        const int customerId = 99999;

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Müşteri bulunamadı.");
    }

    [Fact]
    public async Task GetById_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Customers/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}