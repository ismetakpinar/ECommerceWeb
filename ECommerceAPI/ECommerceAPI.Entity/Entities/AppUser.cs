using ECommerceAPI.Entity.Common;

public class AppUser : BaseEntity
{
    public int AppUserId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpireDate { get; set; }

    public Customer? Customer { get; set; }
}