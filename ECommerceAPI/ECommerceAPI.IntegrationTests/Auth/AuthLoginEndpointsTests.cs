using ECommerceAPI.Dto.Dtos;
using ECommerceAPI.Dto.Dtos.AuthDtos;
using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Auth;

public class AuthLoginEndpointsTests
    : IntegrationTestBase
{
    public AuthLoginEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Login_Should_Return_Ok_When_Credentials_Are_Valid()
    {
        // Arrange
        var request = new LoginDto
        {
            Email = TestConstants.AdminEmail,
            Password = TestConstants.AdminPassword
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Auth/login",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<LoginResponseDto>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Giriş başarılı.");

        result.Data.Should().NotBeNull();

        var token = result.Data!;

        token.AccessToken.Should().NotBeNullOrWhiteSpace();
        token.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_Should_Return_BadRequest_When_Email_Is_Invalid()
    {
        // Arrange
        var request = new LoginDto
        {
            Email = "yanlis@mail.com",
            Password = TestConstants.AdminPassword
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Auth/login",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Email veya şifre hatalı.");
    }

    [Fact]
    public async Task Login_Should_Return_BadRequest_When_Password_Is_Invalid()
    {
        // Arrange
        var request = new LoginDto
        {
            Email = TestConstants.AdminEmail,
            Password = "WrongPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Auth/login",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Email veya şifre hatalı.");
    }

    [Fact]
    public async Task Login_Should_Return_BadRequest_When_Model_Is_Invalid()
    {
        // Arrange
        var request = new LoginDto
        {
            Email = string.Empty,
            Password = string.Empty
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Auth/login",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
    }
}