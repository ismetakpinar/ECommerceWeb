using ECommerceAPI.Dto.Dtos.CustomerDtos;
using FluentValidation;

namespace ECommerceAPI.Business.Validations.CustomerValidators;

public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerDto>
{
    public UpdateCustomerValidator()
    {


        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad alanı zorunludur.")
            .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad alanı zorunludur.")
            .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir.");



        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon alanı zorunludur.")
            .MaximumLength(20).WithMessage("Telefon en fazla 20 karakter olabilir.");
    }
}