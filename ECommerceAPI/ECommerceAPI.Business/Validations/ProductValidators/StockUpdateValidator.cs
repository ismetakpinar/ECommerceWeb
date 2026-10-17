using ECommerceAPI.Dto.Dtos.ProductDtos;
using FluentValidation;

namespace ECommerceAPI.Business.Validations.ProductValidators;

public class StockUpdateValidator : AbstractValidator<StockUpdateDto>
{
    public StockUpdateValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Stok miktarı 0'dan büyük olmalıdır.");
    }
}