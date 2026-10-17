using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ECommerceAPI.Configurations;

public class ConfigureSwaggerOptions
    : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider
        _apiVersionDescriptionProvider;

    public ConfigureSwaggerOptions(
        IApiVersionDescriptionProvider apiVersionDescriptionProvider)
    {
        _apiVersionDescriptionProvider =
            apiVersionDescriptionProvider;
    }

    public void Configure(
        SwaggerGenOptions options)
    {
        foreach (var description
                 in _apiVersionDescriptionProvider
                     .ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                CreateOpenApiInfo(description));
        }
    }

    private static OpenApiInfo CreateOpenApiInfo(
        ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = "ECommerce API",
            Version = description.ApiVersion.ToString(),
            Description =
                "ECommerce projesi için geliştirilen Web API."
        };

        if (description.IsDeprecated)
        {
            info.Description +=
                " Bu API sürümü kullanımdan kaldırılmıştır.";
        }

        return info;
    }
}