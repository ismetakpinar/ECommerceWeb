using ECommerceAPI.Dto.Dtos.ProductDtos;
using FluentValidation;

namespace ECommerceAPI.Business.Validations.ProductValidators;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş bırakılamaz.")
            .MinimumLength(3).WithMessage("Ürün adı en az 3 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Ürün adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Ürün açıklaması boş bırakılamaz.")
            .MaximumLength(1000).WithMessage("Ürün açıklaması en fazla 1000 karakter olabilir.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stok negatif olamaz.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Geçerli bir kategori seçiniz.");
        RuleFor(x => x.ImageUrl)
            .NotEmpty()
            .WithMessage("Ürün görseli zorunludur.")
            .MaximumLength(500)
            .WithMessage("Ürün görsel yolu en fazla 500 karakter olabilir.");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(300)
            .WithMessage("Kısa açıklama en fazla 300 karakter olabilir.");
    }
}