using ECommerceAPI.Business.Abstract;
using ECommerceAPI.DataAccess.UnitOfWorks;

namespace ECommerceAPI.Business.Concrete;

public class AppUserService : IAppUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public AppUserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AppUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<AppUser>();

        return await repository.GetAsync(
            x => x.Email == email,
            cancellationToken);
    }

    public async Task<AppUser?> GetByRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<AppUser>();

        return await repository.GetAsync(
            x => x.RefreshToken == refreshToken,
            cancellationToken);
    }

    public async Task UpdateAsync(
        AppUser appUser,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<AppUser>();

        repository.Update(appUser);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}