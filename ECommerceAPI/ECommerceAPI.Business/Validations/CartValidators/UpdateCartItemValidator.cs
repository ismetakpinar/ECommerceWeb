using ECommerceAPI.Dto.Dtos.CartDtos;
using FluentValidation;

namespace ECommerceAPI.Business.Validations.CartValidators;

public class UpdateCartItemValidator : AbstractValidator<UpdateCartItemDto>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("Ürün ID 0'dan büyük olmalıdır.");

        RuleFor(x => x.Quantity)
    .GreaterThanOrEqualTo(0)
    .WithMessage("Ürün miktarı negatif olamaz.");
    }
}