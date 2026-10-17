using ECommerceAPI.Business.Concrete;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ECommerceAPI.Tests.Services;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _configurationMock =
            new Mock<IConfiguration>();

        _configurationMock
            .Setup(x => x["Jwt:Key"])
            .Returns("ThisIsAVeryStrongSecretKeyForUnitTesting123456");

        _configurationMock
            .Setup(x => x["Jwt:Issuer"])
            .Returns("ECommerceAPI");

        _configurationMock
            .Setup(x => x["Jwt:Audience"])
            .Returns("ECommerceAPIUsers");

        _configurationMock
            .Setup(x => x["Jwt:ExpireMinutes"])
            .Returns("60");

        _jwtService =
            new JwtService(_configurationMock.Object);
    }

    private static AppUser CreateUser()
    {
        return new AppUser
        {
            AppUserId = 15,
            Email = "ismet@example.com",
            Role = "Admin",
            PasswordHash = "hash"
        };
    }

    [Fact]
    public void GenerateToken_ValidUser_ReturnsToken()
    {
        var token =
            _jwtService.GenerateToken(CreateUser());

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwtToken()
    {
        var token =
            _jwtService.GenerateToken(CreateUser());

        var handler =
            new JwtSecurityTokenHandler();

        handler.CanReadToken(token)
            .Should().BeTrue();
    }

    [Fact]
    public void GenerateToken_ContainsCorrectEmailClaim()
    {
        var token =
            _jwtService.GenerateToken(CreateUser());

        var jwt =
            new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        jwt.Claims
            .First(x => x.Type == ClaimTypes.Email)
            .Value
            .Should()
            .Be("ismet@example.com");
    }

    [Fact]
    public void GenerateToken_ContainsCorrectRoleClaim()
    {
        var token =
            _jwtService.GenerateToken(CreateUser());

        var jwt =
            new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        jwt.Claims
            .First(x => x.Type == ClaimTypes.Role)
            .Value
            .Should()
            .Be("Admin");
    }

    [Fact]
    public void GenerateToken_ContainsCorrectUserIdClaim()
    {
        var token =
            _jwtService.GenerateToken(CreateUser());

        var jwt =
            new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        jwt.Claims
            .First(x => x.Type == ClaimTypes.NameIdentifier)
            .Value
            .Should()
            .Be("15");
    }

    [Fact]
    public void GenerateToken_HasCorrectIssuer()
    {
        var token =
            _jwtService.GenerateToken(CreateUser());

        var jwt =
            new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        jwt.Issuer
            .Should()
            .Be("ECommerceAPI");
    }

    [Fact]
    public void GenerateToken_HasCorrectAudience()
    {
        var token =
            _jwtService.GenerateToken(CreateUser());

        var jwt =
            new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        jwt.Audiences
            .Should()
            .ContainSingle();

        jwt.Audiences.First()
            .Should()
            .Be("ECommerceAPIUsers");
    }

    [Fact]
    public void GenerateToken_HasExpirationTime()
    {
        var before =
            DateTime.UtcNow;

        var token =
            _jwtService.GenerateToken(CreateUser());

        var after =
            DateTime.UtcNow;

        var jwt =
            new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        jwt.ValidTo.Should()
            .BeAfter(before.AddMinutes(59));

        jwt.ValidTo.Should()
            .BeBefore(after.AddMinutes(61));
    }

    [Fact]
    public void GenerateToken_UsesHmacSha256Algorithm()
    {
        var token =
            _jwtService.GenerateToken(CreateUser());

        var jwt =
            new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        jwt.SignatureAlgorithm
            .Should()
            .Be(SecurityAlgorithms.HmacSha256);
    }

    [Fact]
    public void GenerateToken_CreatesThreeCustomClaims()
    {
        // Arrange
        var token = _jwtService.GenerateToken(CreateUser());

        // Act
        var jwt = new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        // Assert
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Email);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role);

        jwt.Claims.Count(c =>
            c.Type == ClaimTypes.NameIdentifier ||
            c.Type == ClaimTypes.Email ||
            c.Type == ClaimTypes.Role)
            .Should().Be(3);
    }
}