using ECommerceAPI.Dto.Dtos.CustomerDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Customer;

public class CustomerMeEndpointsTests
    : IntegrationTestBase
{
    public CustomerMeEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetMe_Should_Return_Ok_When_User_Is_Authenticated()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Customers/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<CustomerDetailDto>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Müşteri bilgileri başarıyla getirildi.");

        result.Data.Should().NotBeNull();

        var customer = result.Data!;

        customer.Email.Should().Be(TestConstants.AdminEmail);
    }

    [Fact]
    public async Task GetMe_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Customers/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateMe_Should_Return_Ok_When_Request_Is_Valid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var request = new UpdateCustomerDto
        {
            FirstName = "Updated",
            LastName = "Customer",
            Phone = "05551112233"
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Customers/me",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<string>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Müşteri başarıyla güncellendi.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task UpdateMe_Should_Return_BadRequest_When_Model_Is_Invalid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var request = new UpdateCustomerDto
        {
            FirstName = "",
            LastName = "",
            Phone = ""
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Customers/me",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateMe_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        var request = new UpdateCustomerDto
        {
            FirstName = "Test",
            LastName = "User",
            Phone = "05550000000"
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"{ApiVersion}/Customers/me",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}