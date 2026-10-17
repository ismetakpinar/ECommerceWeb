using ECommerceAPI.Entity.Common;
using ECommerceAPI.Entity.Entities;

public class Customer : BaseEntity
{
    public int CustomerId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int AppUserId { get; set; }

    public string Phone { get; set; } = null!;
    public AppUser AppUser { get; set; } = null!;

    public Cart? Cart { get; set; }

    public ICollection<Address> Addresses { get; set; }
        = new List<Address>();

    public ICollection<Order> Orders { get; set; }
        = new List<Order>();
}