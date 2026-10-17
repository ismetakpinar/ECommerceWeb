using ECommerceAPI.Dto.Dtos.OrderDtos;
using FluentValidation;

namespace ECommerceAPI.Business.Validations.OrderValidators;

public class UpdateOrderValidator : AbstractValidator<UpdateOrderDto>
{
    public UpdateOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0)
            .WithMessage("Geçerli bir sipariş seçiniz.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Geçerli bir sipariş durumu seçiniz.");
    }
}