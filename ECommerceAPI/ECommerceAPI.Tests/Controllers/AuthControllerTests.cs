using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Controllers;
using ECommerceAPI.Dto.Dtos;
using ECommerceAPI.Dto.Dtos.AuthDtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ECommerceAPI.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();

        _controller = new AuthController(
            _authServiceMock.Object);
    }

    [Fact]
    public async Task Register_ReturnsOk()
    {
        // Arrange
        var dto = new RegisterDto
        {
            FirstName = "İsmet",
            LastName = "Akpınar",
            Email = "ismet@test.com",
            Password = "Password123*",
            Phone = "05555555555"
        };

        // Act
        var result = await _controller.Register(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _authServiceMock.Verify(
            x => x.RegisterAsync(
                dto,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Login_ReturnsOk()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "ismet@test.com",
            Password = "Password123*"
        };

        var response = new LoginResponseDto
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(
                dto,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Login(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _authServiceMock.Verify(
            x => x.LoginAsync(
                dto,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void Secret_ReturnsOk()
    {
        // Act
        var result = _controller.Secret();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public void Admin_ReturnsOk()
    {
        // Act
        var result = _controller.Admin();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetCurrentUser_ReturnsOk()
    {
        // Act
        var result = _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RefreshToken_ReturnsOk()
    {
        // Arrange
        var request = new RefreshTokenRequestDto
        {
            RefreshToken = "refresh-token"
        };

        var response = new LoginResponseDto
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token"
        };

        _authServiceMock
            .Setup(x => x.RefreshTokenAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _authServiceMock.Verify(
            x => x.RefreshTokenAsync(
                request,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Logout_ReturnsOk()
    {
        // Arrange
        var request = new RefreshTokenRequestDto
        {
            RefreshToken = "refresh-token"
        };

        // Act
        var result = await _controller.Logout(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _authServiceMock.Verify(
            x => x.LogoutAsync(
                request,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Register_ReturnsCorrectApiResponse()
    {
        var dto = new RegisterDto
        {
            FirstName = "İsmet",
            LastName = "Akpınar",
            Email = "ismet@test.com",
            Password = "Password123*",
            Phone = "05555555555"
        };

        var result = await _controller.Register(dto);

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<string>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Kullanıcı başarıyla kaydedildi.");
    }

    [Fact]
    public async Task Login_ReturnsCorrectApiResponse()
    {
        var dto = new LoginDto
        {
            Email = "ismet@test.com",
            Password = "Password123*"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(
                dto,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResponseDto
            {
                AccessToken = "access",
                RefreshToken = "refresh"
            });

        var result = await _controller.Login(dto);

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<LoginResponseDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Giriş başarılı.");
        response.Data!.AccessToken.Should().Be("access");
    }

    [Fact]
    public void Secret_ReturnsCorrectApiResponse()
    {
        var result = _controller.Secret();

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<string>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("İşlem başarılı.");
        response.Data.Should().Be("Token geçerli. Bu alan korumalıdır.");
    }

    [Fact]
    public void Admin_ReturnsCorrectApiResponse()
    {
        var result = _controller.Admin();

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<string>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("İşlem başarılı.");
        response.Data.Should().Be("Sadece Admin erişebilir.");
    }

    [Fact]
    public void GetCurrentUser_ReturnsCorrectApiResponse()
    {
        var result = _controller.GetCurrentUser();

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<string>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("İşlem başarılı.");
        response.Data.Should().Be("Sadece User erişebilir.");
    }

    [Fact]
    public async Task RefreshToken_ReturnsCorrectApiResponse()
    {
        var request = new RefreshTokenRequestDto
        {
            RefreshToken = "refresh-token"
        };

        _authServiceMock
            .Setup(x => x.RefreshTokenAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResponseDto
            {
                AccessToken = "new-access",
                RefreshToken = "new-refresh"
            });

        var result = await _controller.RefreshToken(request);

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<LoginResponseDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Token yenilendi.");
        response.Data!.AccessToken.Should().Be("new-access");
    }

    [Fact]
    public async Task Logout_ReturnsCorrectApiResponse()
    {
        var request = new RefreshTokenRequestDto
        {
            RefreshToken = "refresh-token"
        };

        var result = await _controller.Logout(request);

        var ok = result as OkObjectResult;
        var response = ok!.Value as ApiResponse<string>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Çıkış başarılı.");
    }
}