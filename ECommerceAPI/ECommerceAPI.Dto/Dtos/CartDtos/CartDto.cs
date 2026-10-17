namespace ECommerceAPI.Dto.Dtos.CartDtos;

public class CartDto
{
    public int CartId { get; set; }

    public int TotalItemCount { get; set; }

    public decimal TotalAmount { get; set; }

    public List<CartItemDto> Items { get; set; } = new();

}
