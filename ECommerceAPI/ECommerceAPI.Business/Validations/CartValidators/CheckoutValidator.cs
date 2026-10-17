using ECommerceAPI.Dto.Dtos.CartDtos;
using FluentValidation;

namespace ECommerceAPI.Business.Validations.CartValidators;

public class CheckoutValidator : AbstractValidator<CheckoutDto>
{
    public CheckoutValidator()
    {
        RuleFor(x => x.AddressId)
            .GreaterThan(0)
            .WithMessage("Adres ID 0'dan büyük olmalıdır.");
    }
}