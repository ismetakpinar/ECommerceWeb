using ECommerceAPI.Business.Concrete;
using ECommerceAPI.DataAccess.UnitOfWorks;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;

namespace ECommerceAPI.Tests.Services;

public class AppUserServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<AppUser>> _appUserRepositoryMock;
    private readonly AppUserService _appUserService;

    public AppUserServiceTests()
    {
        _unitOfWorkMock =
            new Mock<IUnitOfWork>();

        _appUserRepositoryMock =
            new Mock<IGenericRepository<AppUser>>();

        _unitOfWorkMock
            .Setup(x => x.Repository<AppUser>())
            .Returns(_appUserRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _appUserService = new AppUserService(
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetByEmailAsync_UserExists_ReturnsUser()
    {
        // Arrange
        const string email = "ismet@example.com";

        var appUser = new AppUser
        {
            AppUserId = 1,
            Email = email,
            PasswordHash = "hashed-password",
            Role = "User"
        };

        _appUserRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<AppUser, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(appUser);

        // Act
        var result =
            await _appUserService.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(appUser);
        result!.AppUserId.Should().Be(1);
        result.Email.Should().Be(email);

        _appUserRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<Expression<Func<AppUser, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        const string email = "missing@example.com";

        _appUserRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<AppUser, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser?)null);

        // Act
        var result =
            await _appUserService.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();

        _appUserRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<Expression<Func<AppUser, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_PassesCorrectEmailPredicate()
    {
        // Arrange
        const string requestedEmail =
            "ismet@example.com";

        Expression<Func<AppUser, bool>>?
            capturedPredicate = null;

        _appUserRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<AppUser, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<
                Expression<Func<AppUser, bool>>,
                CancellationToken>(
                (predicate, _) =>
                    capturedPredicate = predicate)
            .ReturnsAsync((AppUser?)null);

        // Act
        await _appUserService
            .GetByEmailAsync(requestedEmail);

        // Assert
        capturedPredicate.Should().NotBeNull();

        var predicate =
            capturedPredicate!.Compile();

        var matchingUser = new AppUser
        {
            Email = requestedEmail,
            PasswordHash = "hash",
            Role = "User"
        };

        var differentUser = new AppUser
        {
            Email = "other@example.com",
            PasswordHash = "hash",
            Role = "User"
        };

        predicate(matchingUser).Should().BeTrue();
        predicate(differentUser).Should().BeFalse();
    }

    [Fact]
    public async Task GetByRefreshTokenAsync_UserExists_ReturnsUser()
    {
        // Arrange
        const string refreshToken =
            "valid-refresh-token";

        var appUser = new AppUser
        {
            AppUserId = 2,
            Email = "ismet@example.com",
            PasswordHash = "hashed-password",
            Role = "User",
            RefreshToken = refreshToken,
            RefreshTokenExpireDate =
                DateTime.UtcNow.AddDays(7)
        };

        _appUserRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<AppUser, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(appUser);

        // Act
        var result =
            await _appUserService
                .GetByRefreshTokenAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(appUser);
        result!.RefreshToken.Should()
            .Be(refreshToken);

        _appUserRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<Expression<Func<AppUser, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByRefreshTokenAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        const string refreshToken =
            "invalid-refresh-token";

        _appUserRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<AppUser, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser?)null);

        // Act
        var result =
            await _appUserService
                .GetByRefreshTokenAsync(refreshToken);

        // Assert
        result.Should().BeNull();

        _appUserRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<Expression<Func<AppUser, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByRefreshTokenAsync_PassesCorrectRefreshTokenPredicate()
    {
        // Arrange
        const string requestedRefreshToken =
            "requested-refresh-token";

        Expression<Func<AppUser, bool>>?
            capturedPredicate = null;

        _appUserRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<AppUser, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<
                Expression<Func<AppUser, bool>>,
                CancellationToken>(
                (predicate, _) =>
                    capturedPredicate = predicate)
            .ReturnsAsync((AppUser?)null);

        // Act
        await _appUserService
            .GetByRefreshTokenAsync(
                requestedRefreshToken);

        // Assert
        capturedPredicate.Should().NotBeNull();

        var predicate =
            capturedPredicate!.Compile();

        var matchingUser = new AppUser
        {
            Email = "matching@example.com",
            PasswordHash = "hash",
            Role = "User",
            RefreshToken =
                requestedRefreshToken
        };

        var differentUser = new AppUser
        {
            Email = "different@example.com",
            PasswordHash = "hash",
            Role = "User",
            RefreshToken =
                "different-refresh-token"
        };

        predicate(matchingUser).Should().BeTrue();
        predicate(differentUser).Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ValidUser_UpdatesUserAndSavesChanges()
    {
        // Arrange
        var appUser = new AppUser
        {
            AppUserId = 3,
            Email = "ismet@example.com",
            PasswordHash = "hashed-password",
            Role = "User",
            RefreshToken = "new-refresh-token",
            RefreshTokenExpireDate =
                DateTime.UtcNow.AddDays(7)
        };

        // Act
        await _appUserService.UpdateAsync(appUser);

        // Assert
        _appUserRepositoryMock.Verify(
            x => x.Update(appUser),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_SaveChangesThrowsException_PropagatesException()
    {
        // Arrange
        var appUser = new AppUser
        {
            AppUserId = 4,
            Email = "ismet@example.com",
            PasswordHash = "hashed-password",
            Role = "Admin"
        };

        var expectedException =
            new InvalidOperationException(
                "Veritabanı güncelleme hatası.");

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        Func<Task> act = async () =>
            await _appUserService.UpdateAsync(appUser);

        // Assert
        var exception = await act.Should()
            .ThrowAsync<InvalidOperationException>();

        exception.Which.Should()
            .BeSameAs(expectedException);

        _appUserRepositoryMock.Verify(
            x => x.Update(appUser),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Methods_WhenCancellationTokenProvided_PassTokenToDependencies()
    {
        // Arrange
        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var appUser = new AppUser
        {
            AppUserId = 5,
            Email = "ismet@example.com",
            PasswordHash = "hash",
            Role = "User",
            RefreshToken = "refresh-token"
        };

        _appUserRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<AppUser, bool>>>(),
                cancellationToken))
            .ReturnsAsync(appUser);

        // Act
        await _appUserService.GetByEmailAsync(
            appUser.Email,
            cancellationToken);

        await _appUserService.GetByRefreshTokenAsync(
            appUser.RefreshToken!,
            cancellationToken);

        await _appUserService.UpdateAsync(
            appUser,
            cancellationToken);

        // Assert
        _appUserRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<Expression<Func<AppUser, bool>>>(),
                cancellationToken),
            Times.Exactly(2));

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                cancellationToken),
            Times.Once);
    }
}