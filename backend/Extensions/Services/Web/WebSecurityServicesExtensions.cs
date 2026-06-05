using Microsoft.AspNetCore.HttpOverrides;

namespace MeuCrudCsharp.Extensions.Services.Web;

public static class WebSecurityServicesExtensions
{
    public const string CorsPolicyName = "_myAllowSpecificOrigins";

    public static WebApplicationBuilder AddWebSecurity(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            // Certifique-se de importar o namespace do GeneralSettings
            var generalSettings = builder
                .Configuration.GetSection("General")
                .Get<GeneralSettings>();

            if (generalSettings is null || string.IsNullOrEmpty(generalSettings.BaseUrl))
            {
                throw new InvalidOperationException(
                    "Configurações do General não encontradas ou o BaseUrl está vazio."
                );
            }

            options.AddPolicy(
                name: CorsPolicyName,
                policy =>
                {
                    policy
                        .WithOrigins(
                            generalSettings.BaseUrl,
                            "http://localhost:5045",
                            "http://localhost:5173"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            );
        });

        // Configuração de Proxy para garantir leitura correta de IPs atrás do Nginx
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        return builder;
    }
}
