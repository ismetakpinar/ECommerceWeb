using ECommerceAPI.Entity.Enums;

namespace ECommerceAPI.Dto.Dtos.OrderDtos
{
    public class OrderListDto
    {
        public int OrderId { get; set; }

        public DateTime OrderDate { get; set; }

        public OrderStatus Status { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }
    }
}