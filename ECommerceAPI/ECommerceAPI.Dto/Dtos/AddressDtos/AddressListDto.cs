namespace ECommerceAPI.Dto.Dtos.AddressDtos;

public class AddressListDto
{
    public int AddressId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;
}