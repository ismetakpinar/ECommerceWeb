using ECommerceAPI.Entity.Common;

namespace ECommerceAPI.Entity.Entities;

public class OrderDetail : BaseEntity
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public string ProductName { get; set; } = null!;
}