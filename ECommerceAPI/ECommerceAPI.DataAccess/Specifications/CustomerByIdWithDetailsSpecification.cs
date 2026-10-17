using ECommerceAPI.DataAccess.Specifications;

namespace ECommerceAPI.Business.Specifications.CustomerSpecifications;

public class CustomerByIdWithDetailsSpecification
    : BaseSpecification<Customer>
{
    public CustomerByIdWithDetailsSpecification(int customerId)
        : base(x => x.CustomerId == customerId)
    {
        AddInclude(x => x.Addresses);
        AddInclude(x => x.Orders);
        AddInclude(x => x.AppUser);
    }
}