using ECommerceAPI.Entity.Common;
using System.ComponentModel.DataAnnotations;
namespace ECommerceAPI.Entity.Entities;

public class Product : BaseEntity
{
    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;


    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string Description { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;
    public string ImageUrl { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public bool IsFeatured { get; set; } = false;

}