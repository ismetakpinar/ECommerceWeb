using ECommerceAPI.DataAccess.Specifications;
using ECommerceAPI.Entity.Entities;

public class OrderByIdWithCustomerSpecification
    : BaseSpecification<Order>
{
    public OrderByIdWithCustomerSpecification(int id)
        : base(x => x.OrderId == id)
    {
        AddInclude(x => x.Customer);
    }
}