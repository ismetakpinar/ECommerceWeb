using ECommerceAPI.DataAccess.Specifications;
using ECommerceAPI.Entity.Entities;

public class GetMyOrdersSpecification
    : BaseSpecification<Order>
{
    public GetMyOrdersSpecification(int customerId)
        : base(x => x.CustomerId == customerId)
    {
        ApplyOrderByDescending(x => x.OrderDate);
    }
}