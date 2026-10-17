namespace ECommerce.Web.ViewModels.Order;

public class OrderDetailViewModel
{
    public int OrderId { get; set; }

    public DateTime OrderDate { get; set; }

    public int Status { get; set; }

    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public List<OrderItemViewModel> Items { get; set; } = [];

    public string ShippingAddressTitle { get; set; } = string.Empty;

    public string ShippingCity { get; set; } = string.Empty;

    public string ShippingDistrict { get; set; } = string.Empty;

    public string ShippingFullAddress { get; set; } = string.Empty;
}