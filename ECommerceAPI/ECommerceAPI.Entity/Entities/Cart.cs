using ECommerceAPI.Entity.Common;

public class Cart : BaseEntity
{
    public int CartId { get; set; }

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public ICollection<CartItem> CartItems { get; set; }
        = new List<CartItem>();
}