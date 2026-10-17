using ECommerceAPI.Entity.Common;
namespace ECommerceAPI.Entity.Entities;

public class Address : BaseEntity
{
    public int AddressId { get; set; }

    public string City { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public string FullAddress { get; set; } = string.Empty;

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;
}