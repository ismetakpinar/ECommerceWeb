using ECommerce.Web.ViewModels.Address;
using ECommerce.Web.ViewModels.Cart;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.ViewModels.Checkout;

public sealed class CheckoutViewModel
{
    [Required(ErrorMessage = "Lütfen bir adres seçiniz.")]
    [Display(Name = "Teslimat Adresi")]
    public int AddressId { get; set; }

    public string? Note { get; set; }
    public List<AddressViewModel> Addresses { get; set; }
        = new();
    public CartViewModel Cart { get; set; } = new();
}