namespace ECommerceAPI.Dto.Dtos.AddressDtos;

public class UpdateAddressDto
{
    public int AddressId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public string FullAddress { get; set; } = string.Empty;


}