using ECommerceAPI.Entity.Entities;

namespace ECommerceAPI.Business.Abstract;

public interface IJwtService
{
    string GenerateToken(AppUser user);
}