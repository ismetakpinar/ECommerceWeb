using ECommerceAPI.Entity.Entities;

namespace ECommerceAPI.DataAccess.Specifications;

public class OrderListSpecification : BaseSpecification<Order>

{
    public OrderListSpecification()
    {
        AddInclude(x => x.Customer);
    }
}
