using ECommerceAPI.Dto.Dtos;
using ECommerceAPI.Dto.Dtos.AuthDtos;

namespace ECommerceAPI.Business.Abstract;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(
        LoginDto loginDto,
        CancellationToken cancellationToken = default);

    Task<LoginResponseDto> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default);

    Task RegisterAsync(
        RegisterDto registerDto,
        CancellationToken cancellationToken = default);
}