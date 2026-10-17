using ECommerceAPI.Dto.Dtos.CategoryDtos;
using FluentValidation;

namespace ECommerceAPI.Business.Validations.CategoryValidators;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDto>

{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Geçerli bir kategori ID değeri girilmelidir.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Kategori adı boş bırakılamaz.")
            .MinimumLength(2)
            .WithMessage("Kategori adı en az 2 karakter olmalıdır.")
            .MaximumLength(100)
            .WithMessage("Kategori adı en fazla 100 karakter olabilir.");
    }
}
