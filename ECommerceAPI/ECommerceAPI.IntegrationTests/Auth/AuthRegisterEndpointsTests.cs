using ECommerceAPI.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceAPI.IntegrationTests.Auth;

public class AuthRegisterEndpointsTests
    : IntegrationTestBase
{
    public AuthRegisterEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Register_Should_Return_Ok_When_Request_Is_Valid()
    {
        // Arrange
        var request = new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"test{Guid.NewGuid():N}@mail.com",
            Password = "Test123!",
            Phone = "05555555555"
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Auth/register",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await ReadResponseAsync<string>(response);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Kullanıcı başarıyla kaydedildi.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Register_Should_Return_BadRequest_When_Email_Already_Exists()
    {
        // Arrange
        var request = new RegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = TestConstants.AdminEmail,
            Password = "Test123!",
            Phone = "05555555555"
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Auth/register",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Bu email adresi zaten kullanılıyor.");
    }

    [Fact]
    public async Task Register_Should_Return_BadRequest_When_Model_Is_Invalid()
    {
        // Arrange
        var request = new RegisterDto
        {
            FirstName = "",
            LastName = "",
            Email = "",
            Password = "",
            Phone = ""
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiVersion}/Auth/register",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result =
            await ReadResponseAsync<object>(response);

        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
    }
}