namespace ECommerceAPI.Business.Abstract;

public interface IAppUserService
{
    Task<AppUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<AppUser?> GetByRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        AppUser appUser,
        CancellationToken cancellationToken = default);
}