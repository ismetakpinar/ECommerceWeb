using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Concrete;
using ECommerceAPI.DataAccess.UnitOfWorks;
using ECommerceAPI.Dto.Dtos;
using ECommerceAPI.Dto.Dtos.AuthDtos;
using ECommerceAPI.Entity.Exceptions;
using FluentAssertions;
using Moq;
using System.Security.Cryptography;
using System.Text;

namespace ECommerceAPI.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IAppUserService> _appUserServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly Mock<IGenericRepository<AppUser>>
        _appUserRepositoryMock;

    private readonly Mock<IGenericRepository<Customer>>
        _customerRepositoryMock;

    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _appUserServiceMock =
            new Mock<IAppUserService>();

        _jwtServiceMock =
            new Mock<IJwtService>();

        _unitOfWorkMock =
            new Mock<IUnitOfWork>();

        _appUserRepositoryMock =
            new Mock<IGenericRepository<AppUser>>();

        _customerRepositoryMock =
            new Mock<IGenericRepository<Customer>>();

        _unitOfWorkMock
            .Setup(x => x.Repository<AppUser>())
            .Returns(_appUserRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.Repository<Customer>())
            .Returns(_customerRepositoryMock.Object);

        _authService = new AuthService(
            _appUserServiceMock.Object,
            _jwtServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_EmailIsAvailable_AddsUserAndCustomerAndSavesChanges()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            FirstName = "  İsmet  ",
            LastName = "  Akpınar  ",
            Email = "  ismet@example.com  ",
            Password = "Test123!",
            Phone = "  05000000000  "
        };

        AppUser? addedUser = null;
        Customer? addedCustomer = null;

        _appUserServiceMock
            .Setup(x => x.GetByEmailAsync(
                "ismet@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser?)null);

        _appUserRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<AppUser>(),
                It.IsAny<CancellationToken>()))
            .Callback<AppUser, CancellationToken>(
                (user, _) => addedUser = user)
            .Returns(Task.CompletedTask);

        _customerRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()))
            .Callback<Customer, CancellationToken>(
                (customer, _) => addedCustomer = customer)
            .Returns(Task.CompletedTask);

        // Act
        await _authService.RegisterAsync(registerDto);

        // Assert
        registerDto.FirstName.Should().Be("İsmet");
        registerDto.LastName.Should().Be("Akpınar");
        registerDto.Email.Should().Be("ismet@example.com");
        registerDto.Phone.Should().Be("05000000000");

        addedUser.Should().NotBeNull();
        addedUser!.Email.Should().Be("ismet@example.com");
        addedUser.Role.Should().Be("User");

        BCrypt.Net.BCrypt.Verify(
                registerDto.Password,
                addedUser.PasswordHash)
            .Should()
            .BeTrue();

        addedCustomer.Should().NotBeNull();
        addedCustomer!.FirstName.Should().Be("İsmet");
        addedCustomer.LastName.Should().Be("Akpınar");
        addedCustomer.Phone.Should().Be("05000000000");
        addedCustomer.AppUser.Should().BeSameAs(addedUser);

        _appUserServiceMock.Verify(
            x => x.GetByEmailAsync(
                "ismet@example.com",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _appUserRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<AppUser>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _customerRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ThrowsBadRequestException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            FirstName = "  İsmet  ",
            LastName = "  Akpınar  ",
            Email = "  ismet@example.com  ",
            Password = "Test123!",
            Phone = "  05000000000  "
        };

        var existingUser = new AppUser
        {
            Email = "ismet@example.com",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword("Test123!"),
            Role = "User"
        };

        _appUserServiceMock
            .Setup(x => x.GetByEmailAsync(
                "ismet@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () =>
            await _authService.RegisterAsync(registerDto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "Bu email adresi zaten kullanılıyor.");

        registerDto.FirstName.Should().Be("İsmet");
        registerDto.LastName.Should().Be("Akpınar");
        registerDto.Email.Should().Be("ismet@example.com");
        registerDto.Phone.Should().Be("05000000000");

        _appUserRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<AppUser>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _customerRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokensAndUpdatesUser()
    {
        // Arrange
        const string password = "Test123!";
        const string accessToken = "access-token";

        var loginDto = new LoginDto
        {
            Email = "ismet@example.com",
            Password = password
        };

        var user = new AppUser
        {
            Email = loginDto.Email,
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(password),
            Role = "User"
        };

        var beforeLogin = DateTime.UtcNow;

        _appUserServiceMock
            .Setup(x => x.GetByEmailAsync(
                loginDto.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(user))
            .Returns(accessToken);

        // Act
        var result =
            await _authService.LoginAsync(loginDto);

        var afterLogin = DateTime.UtcNow;

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(accessToken);
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();

        user.RefreshToken.Should()
            .Be(HashRefreshToken(result.RefreshToken));

        user.RefreshToken.Should()
            .NotBe(result.RefreshToken);

        user.RefreshTokenExpireDate.Should().NotBeNull();

        user.RefreshTokenExpireDate!.Value.Should()
            .BeOnOrAfter(beforeLogin.AddDays(7));

        user.RefreshTokenExpireDate.Value.Should()
            .BeOnOrBefore(afterLogin.AddDays(7));

        _jwtServiceMock.Verify(
            x => x.GenerateToken(user),
            Times.Once);

        _appUserServiceMock.Verify(
            x => x.UpdateAsync(
                user,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_UserDoesNotExist_ThrowsBadRequestException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "unknown@example.com",
            Password = "Test123!"
        };

        _appUserServiceMock
            .Setup(x => x.GetByEmailAsync(
                loginDto.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser?)null);

        // Act
        Func<Task> act = async () =>
            await _authService.LoginAsync(loginDto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Email veya şifre hatalı.");

        _jwtServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<AppUser>()),
            Times.Never);

        _appUserServiceMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<AppUser>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_PasswordIsInvalid_ThrowsBadRequestException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "ismet@example.com",
            Password = "WrongPassword!"
        };

        var user = new AppUser
        {
            Email = loginDto.Email,
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword("Correct123!"),
            Role = "User"
        };

        _appUserServiceMock
            .Setup(x => x.GetByEmailAsync(
                loginDto.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () =>
            await _authService.LoginAsync(loginDto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Email veya şifre hatalı.");

        _jwtServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<AppUser>()),
            Times.Never);

        _appUserServiceMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<AppUser>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task RefreshTokenAsync_ValidRefreshToken_ReturnsNewTokensAndUpdatesUser()
    {
        // Arrange
        const string oldRefreshToken = "old-refresh-token";
        const string newAccessToken = "new-access-token";

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = oldRefreshToken
        };

        var oldHashedRefreshToken =
            HashRefreshToken(oldRefreshToken);

        var user = new AppUser
        {
            Email = "ismet@example.com",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword("Test123!"),
            Role = "User",
            RefreshToken = oldHashedRefreshToken,
            RefreshTokenExpireDate =
                DateTime.UtcNow.AddDays(1)
        };

        var beforeRefresh = DateTime.UtcNow;

        _appUserServiceMock
            .Setup(x => x.GetByRefreshTokenAsync(
                oldHashedRefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(user))
            .Returns(newAccessToken);

        // Act
        var result =
            await _authService.RefreshTokenAsync(request);

        var afterRefresh = DateTime.UtcNow;

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(newAccessToken);
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe(oldRefreshToken);

        user.RefreshToken.Should()
            .Be(HashRefreshToken(result.RefreshToken));

        user.RefreshToken.Should()
            .NotBe(oldHashedRefreshToken);

        user.RefreshTokenExpireDate.Should().NotBeNull();

        user.RefreshTokenExpireDate!.Value.Should()
            .BeOnOrAfter(beforeRefresh.AddDays(7));

        user.RefreshTokenExpireDate.Value.Should()
            .BeOnOrBefore(afterRefresh.AddDays(7));

        _appUserServiceMock.Verify(
            x => x.GetByRefreshTokenAsync(
                oldHashedRefreshToken,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _jwtServiceMock.Verify(
            x => x.GenerateToken(user),
            Times.Once);

        _appUserServiceMock.Verify(
            x => x.UpdateAsync(
                user,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_TokenDoesNotBelongToUser_ThrowsBadRequestException()
    {
        // Arrange
        const string refreshToken = "invalid-refresh-token";

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = refreshToken
        };

        var hashedRefreshToken =
            HashRefreshToken(refreshToken);

        _appUserServiceMock
            .Setup(x => x.GetByRefreshTokenAsync(
                hashedRefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser?)null);

        // Act
        Func<Task> act = async () =>
            await _authService.RefreshTokenAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Geçersiz refresh token.");

        _jwtServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<AppUser>()),
            Times.Never);

        _appUserServiceMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<AppUser>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_TokenExpiryDateIsNull_ThrowsBadRequestException()
    {
        // Arrange
        const string refreshToken = "refresh-token";

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = refreshToken
        };

        var hashedRefreshToken =
            HashRefreshToken(refreshToken);

        var user = new AppUser
        {
            Email = "ismet@example.com",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword("Test123!"),
            Role = "User",
            RefreshToken = hashedRefreshToken,
            RefreshTokenExpireDate = null
        };

        _appUserServiceMock
            .Setup(x => x.GetByRefreshTokenAsync(
                hashedRefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () =>
            await _authService.RefreshTokenAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "Refresh token süresi dolmuş veya geçersiz.");

        _jwtServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<AppUser>()),
            Times.Never);

        _appUserServiceMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<AppUser>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_TokenIsExpired_ThrowsBadRequestException()
    {
        // Arrange
        const string refreshToken = "expired-refresh-token";

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = refreshToken
        };

        var hashedRefreshToken =
            HashRefreshToken(refreshToken);

        var user = new AppUser
        {
            Email = "ismet@example.com",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword("Test123!"),
            Role = "User",
            RefreshToken = hashedRefreshToken,
            RefreshTokenExpireDate =
                DateTime.UtcNow.AddMinutes(-1)
        };

        _appUserServiceMock
            .Setup(x => x.GetByRefreshTokenAsync(
                hashedRefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () =>
            await _authService.RefreshTokenAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "Refresh token süresi dolmuş veya geçersiz.");

        _jwtServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<AppUser>()),
            Times.Never);

        _appUserServiceMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<AppUser>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_ValidRefreshToken_ClearsTokenAndUpdatesUser()
    {
        // Arrange
        const string refreshToken = "valid-refresh-token";

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = refreshToken
        };

        var hashedRefreshToken =
            HashRefreshToken(refreshToken);

        var user = new AppUser
        {
            Email = "ismet@example.com",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword("Test123!"),
            Role = "User",
            RefreshToken = hashedRefreshToken,
            RefreshTokenExpireDate =
                DateTime.UtcNow.AddDays(7)
        };

        _appUserServiceMock
            .Setup(x => x.GetByRefreshTokenAsync(
                hashedRefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _authService.LogoutAsync(request);

        // Assert
        user.RefreshToken.Should().BeNull();
        user.RefreshTokenExpireDate.Should().BeNull();

        _appUserServiceMock.Verify(
            x => x.GetByRefreshTokenAsync(
                hashedRefreshToken,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _appUserServiceMock.Verify(
            x => x.UpdateAsync(
                user,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_InvalidRefreshToken_ThrowsBadRequestException()
    {
        // Arrange
        const string refreshToken = "invalid-refresh-token";

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = refreshToken
        };

        var hashedRefreshToken =
            HashRefreshToken(refreshToken);

        _appUserServiceMock
            .Setup(x => x.GetByRefreshTokenAsync(
                hashedRefreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser?)null);

        // Act
        Func<Task> act = async () =>
            await _authService.LogoutAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Geçersiz refresh token.");

        _appUserServiceMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<AppUser>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static string HashRefreshToken(
        string refreshToken)
    {
        var tokenBytes =
            Encoding.UTF8.GetBytes(refreshToken);

        var hashBytes =
            SHA256.HashData(tokenBytes);

        return Convert.ToBase64String(hashBytes);
    }
}