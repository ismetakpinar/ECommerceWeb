using ECommerceAPI.Business.Abstract;
using System.Security.Claims;

namespace ECommerceAPI.Business.Concrete;

public class CurrentUserService : ICurrentUserService
{

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetAppUserId()
    {
        var userIdClaim = _httpContextAccessor
            .HttpContext?
            .User?
            .FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null)
        {
            throw new UnauthorizedAccessException(
                "Kullanıcı kimliği bulunamadı.");
        }

        if (!int.TryParse(userIdClaim.Value, out var appUserId))
        {
            throw new UnauthorizedAccessException(
                "Geçersiz kullanıcı kimliği.");
        }

        return appUserId;
    }



}
