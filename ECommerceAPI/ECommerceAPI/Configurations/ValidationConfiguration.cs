using ECommerceAPI.Business.Helpers;
using ECommerceAPI.Business.Validations.ProductValidators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Configurations;

public static class ValidationConfiguration
{
    public static IServiceCollection AddValidationConfiguration(
        this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();

        services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage)
                    .ToList();

                var response = ApiResponse<object>.ErrorResponse(
                    message: "Doğrulama hatası oluştu.",
                    statusCode: StatusCodes.Status400BadRequest,
                    errors: errors);

                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }
}