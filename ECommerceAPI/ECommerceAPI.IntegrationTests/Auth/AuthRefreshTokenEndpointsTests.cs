using ECommerceAPI.Dto.Dtos;
using ECommerceAPI.Dto.Dtos.AuthDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Auth;

public class AuthRefreshTokenEndpointsTests
    : IntegrationTestBase
{
    public AuthRefreshTokenEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task RefreshToken_Should_Return_Ok_When_Request_Is_Valid()
    {
        // Arrange
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
            $"{ApiVersion}/Auth/refresh-token",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<LoginResponseDto>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Token yenilendi.");

        result.Data.Should().NotBeNull();

        var token = result.Data!;

        token.AccessToken.Should().NotBeNullOrWhiteSpace();
        token.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RefreshToken_Should_Return_BadRequest_When_Token_Is_Invalid()
    {
        // Arrange
        var request = new RefreshTokenRequestDto
        {
            RefreshToken = Guid.NewGuid().ToString()
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Auth/refresh-token",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Geçersiz refresh token.");
    }

    [Fact]
    public async Task RefreshToken_Should_Return_BadRequest_When_Model_Is_Invalid()
    {
        // Arrange
        var request = new RefreshTokenRequestDto
        {
            RefreshToken = string.Empty
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Auth/refresh-token",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
    }
}