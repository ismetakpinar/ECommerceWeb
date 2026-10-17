using ECommerceAPI.Business.Abstract;
using ECommerceAPI.DataAccess.UnitOfWorks;
using ECommerceAPI.Dto.Dtos;
using ECommerceAPI.Dto.Dtos.AuthDtos;
using ECommerceAPI.Entity.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace ECommerceAPI.Business.Concrete;

public class AuthService : IAuthService
{
    private const int RefreshTokenLifetimeInDays = 7;

    private readonly IAppUserService _appUserService;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IAppUserService appUserService,
        IJwtService jwtService,
        IUnitOfWork unitOfWork)
    {
        _appUserService = appUserService;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    private static string HashRefreshToken(string refreshToken)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(refreshToken);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToBase64String(hashBytes);
    }

    private static string CreateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(
        AppUser user,
        CancellationToken cancellationToken)
    {
        var refreshToken = CreateRefreshToken();

        user.RefreshToken = HashRefreshToken(refreshToken);
        user.RefreshTokenExpireDate =
            DateTime.UtcNow.AddDays(RefreshTokenLifetimeInDays);

        await _appUserService.UpdateAsync(
            user,
            cancellationToken);

        return refreshToken;
    }

    public async Task<LoginResponseDto> LoginAsync(
        LoginDto loginDto,
        CancellationToken cancellationToken = default)
    {
        var user = await _appUserService.GetByEmailAsync(
            loginDto.Email,
            cancellationToken);

        if (user is null)
            throw new BadRequestException(
                "Email veya şifre hatalı.");

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(
            loginDto.Password,
            user.PasswordHash);

        if (!isPasswordValid)
            throw new BadRequestException(
                "Email veya şifre hatalı.");

        var accessToken = _jwtService.GenerateToken(user);

        var refreshToken =
            await GenerateAndSaveRefreshTokenAsync(
                user,
                cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task LogoutAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var hashedRefreshToken =
            HashRefreshToken(request.RefreshToken);

        var user = await _appUserService.GetByRefreshTokenAsync(
            hashedRefreshToken,
            cancellationToken);

        if (user is null)
            throw new BadRequestException(
                "Geçersiz refresh token.");

        user.RefreshToken = null;
        user.RefreshTokenExpireDate = null;

        await _appUserService.UpdateAsync(
            user,
            cancellationToken);
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var hashedRefreshToken =
            HashRefreshToken(request.RefreshToken);

        var user = await _appUserService.GetByRefreshTokenAsync(
            hashedRefreshToken,
            cancellationToken);

        if (user is null)
            throw new BadRequestException(
                "Geçersiz refresh token.");

        if (user.RefreshTokenExpireDate is null ||
            user.RefreshTokenExpireDate <= DateTime.UtcNow)
        {
            throw new BadRequestException(
                "Refresh token süresi dolmuş veya geçersiz.");
        }

        var accessToken =
            _jwtService.GenerateToken(user);

        var refreshToken =
            await GenerateAndSaveRefreshTokenAsync(
                user,
                cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task RegisterAsync(
        RegisterDto registerDto,
        CancellationToken cancellationToken = default)
    {
        registerDto.Email = registerDto.Email.Trim();
        registerDto.FirstName = registerDto.FirstName.Trim();
        registerDto.LastName = registerDto.LastName.Trim();
        registerDto.Phone = registerDto.Phone.Trim();

        var existingUser =
            await _appUserService.GetByEmailAsync(
                registerDto.Email,
                cancellationToken);

        if (existingUser is not null)
            throw new BadRequestException(
                "Bu email adresi zaten kullanılıyor.");

        var user = new AppUser
        {
            Email = registerDto.Email,
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    registerDto.Password),
            Role = "User"
        };

        var customer = new Customer
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Phone = registerDto.Phone,
            AppUser = user
        };

        await _unitOfWork
            .Repository<AppUser>()
            .AddAsync(user, cancellationToken);

        await _unitOfWork
            .Repository<Customer>()
            .AddAsync(customer, cancellationToken);

        await _unitOfWork
            .SaveChangesAsync(cancellationToken);
    }
}