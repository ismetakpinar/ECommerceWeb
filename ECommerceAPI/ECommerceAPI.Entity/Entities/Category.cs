using ECommerceAPI.Entity.Common;
namespace ECommerceAPI.Entity.Entities;

public class Category : BaseEntity
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}