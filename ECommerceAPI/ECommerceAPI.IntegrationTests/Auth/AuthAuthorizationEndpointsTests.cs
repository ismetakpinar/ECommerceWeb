using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;

namespace ECommerceAPI.IntegrationTests.Auth;

public class AuthAuthorizationEndpointsTests
    : IntegrationTestBase
{
    public AuthAuthorizationEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Secret_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Auth/secret");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Secret_Should_Return_Ok_When_User_Is_Authenticated()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Auth/secret");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<string>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("İşlem başarılı.");
        result.Data.Should().Be("Token geçerli. Bu alan korumalıdır.");
    }

    [Fact]
    public async Task Admin_Should_Return_Ok_When_User_Is_Admin()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Auth/admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<string>(response);

        result.Success.Should().BeTrue();
        result.Data.Should().Be("Sadece Admin erişebilir.");
    }

    [Fact]
    public async Task Admin_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Auth/admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task User_Should_Return_Forbidden_When_Admin_User_Accesses_User_Endpoint()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Auth/user");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task User_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response = await Client.GetAsync(
            $"{ApiVersion}/Auth/user");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}