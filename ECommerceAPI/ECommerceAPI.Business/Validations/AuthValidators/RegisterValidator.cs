using FluentValidation;

namespace ECommerceAPI.Business.Validations.AuthValidators;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("Ad alanı boş bırakılamaz.")
                .MaximumLength(50)
                .WithMessage("Ad en fazla 50 karakter olabilir.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Soyad alanı boş bırakılamaz.")
            .MaximumLength(50)
            .WithMessage("Soyad en fazla 50 karakter olabilir.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email alanı boş bırakılamaz.")
            .EmailAddress()
            .WithMessage("Geçerli bir email adresi giriniz.")
            .MaximumLength(256)
            .WithMessage("Email en fazla 256 karakter olabilir.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Şifre alanı boş bırakılamaz.")
            .MinimumLength(6)
            .WithMessage("Şifre en az 6 karakter olmalıdır.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Telefon alanı boş bırakılamaz.")
            .MaximumLength(20)
            .WithMessage("Telefon en fazla 20 karakter olabilir.");
    }
}