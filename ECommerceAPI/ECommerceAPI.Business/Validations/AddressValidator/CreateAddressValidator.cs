using ECommerceAPI.Dto.Dtos.AddressDtos;
using FluentValidation;

namespace ECommerceAPI.Business.Validators.AddressValidators;

public class CreateAddressValidator : AbstractValidator<CreateAddressDto>
{
    public CreateAddressValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Adres başlığı zorunludur.")
            .MaximumLength(50)
            .WithMessage("Adres başlığı en fazla 50 karakter olabilir.");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("Şehir zorunludur.")
            .MaximumLength(50)
            .WithMessage("Şehir en fazla 50 karakter olabilir.");

        RuleFor(x => x.District)
            .NotEmpty()
            .WithMessage("İlçe zorunludur.")
            .MaximumLength(50)
            .WithMessage("İlçe en fazla 50 karakter olabilir.");

        RuleFor(x => x.FullAddress)
            .NotEmpty()
            .WithMessage("Adres bilgisi zorunludur.")
            .MaximumLength(500)
            .WithMessage("Adres en fazla 500 karakter olabilir.");
    }
}