using ECommerceAPI.Entity.Common;
using ECommerceAPI.Entity.Enums;
namespace ECommerceAPI.Entity.Entities;

public class Order : BaseEntity
{
    public int OrderId { get; set; }

    public DateTime OrderDate { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public string ShippingAddressTitle { get; set; } = string.Empty;

    public string ShippingCity { get; set; } = string.Empty;

    public string ShippingDistrict { get; set; } = string.Empty;

    public string ShippingFullAddress { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}