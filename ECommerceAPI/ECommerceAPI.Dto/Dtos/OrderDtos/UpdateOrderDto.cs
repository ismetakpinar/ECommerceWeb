using ECommerceAPI.Entity.Enums;

namespace ECommerceAPI.Dto.Dtos.OrderDtos
{
    public class UpdateOrderDto
    {
        public int OrderId { get; set; }

        public OrderStatus Status { get; set; }
    }
}