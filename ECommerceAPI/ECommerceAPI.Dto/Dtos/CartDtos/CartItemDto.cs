namespace ECommerceAPI.Dto.Dtos.CartDtos
{
    public class CartItemDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal LineTotal { get; set; }
    }
}
