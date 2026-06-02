using System.Text.Json;

namespace MeuCrudCsharp.Extensions.Services.Presentation;

public static class ApiServicesExtensions
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Garante o padrão camelCase nas respostas JSON da API
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        return builder;
    }
}