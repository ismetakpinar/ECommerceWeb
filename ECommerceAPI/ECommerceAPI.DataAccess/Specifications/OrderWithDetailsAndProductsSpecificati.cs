using ECommerceAPI.Entity.Entities;

namespace ECommerceAPI.DataAccess.Specifications;

public class OrderWithDetailsAndProductsSpecification
    : BaseSpecification<Order>
{
    public OrderWithDetailsAndProductsSpecification(int orderId)
        : base(x => x.OrderId == orderId)
    {
        AddInclude("OrderDetails.Product");
    }
}