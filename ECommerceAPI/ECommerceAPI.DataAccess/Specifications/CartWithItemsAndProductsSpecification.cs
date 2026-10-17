using ECommerceAPI.DataAccess.Specifications;

public class CartWithItemsAndProductsSpecification
    : BaseSpecification<Cart>
{
    public CartWithItemsAndProductsSpecification(int customerId)
        : base(x => x.CustomerId == customerId)
    {
        AddInclude("CartItems.Product");
    }
}