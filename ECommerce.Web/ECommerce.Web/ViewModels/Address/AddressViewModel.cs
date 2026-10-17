namespace ECommerce.Web.ViewModels.Address;

public sealed class AddressViewModel
{
    public int AddressId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public string FullAddress { get; set; } = string.Empty;
}