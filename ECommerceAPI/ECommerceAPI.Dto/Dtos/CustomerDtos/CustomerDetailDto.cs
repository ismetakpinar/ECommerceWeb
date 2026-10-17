namespace ECommerceAPI.Dto.Dtos.CustomerDtos;

public class CustomerDetailDto
{
    public int CustomerId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public int AddressCount { get; set; }

    public int OrderCount { get; set; }
}