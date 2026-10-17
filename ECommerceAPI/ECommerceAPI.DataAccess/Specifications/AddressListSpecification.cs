using ECommerceAPI.DataAccess.Specifications;
using ECommerceAPI.Entity.Entities;

public class AddressListSpecification
    : BaseSpecification<Address>
{
    public AddressListSpecification(int customerId)
        : base(x => x.CustomerId == customerId)
    {
        AddInclude(x => x.Customer);
    }
}