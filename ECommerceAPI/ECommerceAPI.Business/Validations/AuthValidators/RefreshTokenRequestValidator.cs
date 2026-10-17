using ECommerceAPI.Dto.Dtos;
using FluentValidation;

namespace ECommerceAPI.Business.Validations.AuthValidators;

public class RefreshTokenRequestValidator
    : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token boş olamaz.");
    }
}