using ECommerceAPI.Dto.Dtos;
using ECommerceAPI.Dto.Dtos.AuthDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Auth;

public class AuthLogoutEndpointsTests
    : IntegrationTestBase
{
    public AuthLogoutEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Logout_Should_Return_Ok_When_RefreshToken_Is_Valid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var loginResponse = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Auth/login",
            new LoginDto
            {
                Email = TestConstants.AdminEmail,
                Password = TestConstants.AdminPassword
            });

        loginResponse.EnsureSuccessStatusCode();

        var loginResult =
            await ReadResponseAsync<LoginResponseDto>(loginResponse);
        loginResult.Data.Should().NotBeNull();

        var login = loginResult.Data!;

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = login.RefreshToken
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Auth/logout",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<string>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Çıkış başarılı.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Logout_Should_Return_BadRequest_When_RefreshToken_Is_Invalid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = Guid.NewGuid().ToString()
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Auth/logout",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Geçersiz refresh token.");
    }

    [Fact]
    public async Task Logout_Should_Return_BadRequest_When_Model_Is_Invalid()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = string.Empty
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Auth/logout",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
    }

    [Fact]
    public async Task Logout_Should_Return_Unauthorized_When_User_Is_Not_Authenticated()
    {
        // Arrange
        var request = new RefreshTokenRequestDto
        {
            RefreshToken = Guid.NewGuid().ToString()
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Auth/logout",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_Should_Return_Unauthorized_When_AccessToken_Is_Invalid()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                Guid.NewGuid().ToString());

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = Guid.NewGuid().ToString()
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Auth/logout",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}