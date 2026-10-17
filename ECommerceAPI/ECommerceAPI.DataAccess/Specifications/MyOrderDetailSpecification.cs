using ECommerceAPI.Entity.Entities;

namespace ECommerceAPI.DataAccess.Specifications;

public class MyOrderDetailSpecification
    : BaseSpecification<Order>
{
    public MyOrderDetailSpecification(
        int orderId,
        int customerId)
        : base(x =>
            x.OrderId == orderId &&
            x.CustomerId == customerId)
    {
        AddInclude("OrderDetails.Product");
    }
}