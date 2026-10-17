using ECommerceAPI.Dto.Dtos.CartDtos;
using FluentValidation;

namespace ECommerceAPI.Business.Validations.CartValidators;

public class AddToCartValidator : AbstractValidator<AddToCartDto>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("Ürün ID 0'dan büyük olmalıdır.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Ürün miktarı 0'dan büyük olmalıdır.");
    }
}