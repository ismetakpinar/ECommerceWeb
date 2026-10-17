namespace ECommerceAPI.DataAccess.Specifications;

public class CustomerWithDetailsSpecification
    : BaseSpecification<Customer>
{
    public CustomerWithDetailsSpecification(int appUserId)
        : base(x => x.AppUserId == appUserId)
    {
        AddInclude(x => x.AppUser);
        AddInclude(x => x.Addresses);
        AddInclude(x => x.Orders);
    }
}