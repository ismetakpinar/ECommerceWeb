using ECommerceAPI.Business.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerceAPI.Business.Concrete;

public class JwtService : IJwtService
{
    private readonly SigningCredentials _credentials;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly double _expireMinutes;

    public JwtService(IConfiguration configuration)
    {
        _issuer = configuration["Jwt:Issuer"]!;
        _audience = configuration["Jwt:Audience"]!;
        _expireMinutes = Convert.ToDouble(configuration["Jwt:ExpireMinutes"]);

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        _credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);
    }

    public string GenerateToken(AppUser user)
    {
        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.AppUserId.ToString()),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expireMinutes),
            signingCredentials: _credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}