using Microsoft.AspNetCore.HttpOverrides;

namespace MeuCrudCsharp.Extensions.Services.Web;

public static class WebSecurityServicesExtensions
{
    public const string CorsPolicyName = "_myAllowSpecificOrigins";

    public static WebApplicationBuilder AddWebSecurity(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            var generalSettings = builder
                .Configuration.GetSection(GeneralSettings.SectionName)
                .Get<GeneralSettings>() ?? new GeneralSettings();

            generalSettings.BaseUrl ??= builder.Configuration["GENERAL__BASEURL"]
                ?? builder.Configuration["General:BaseUrl"]
                ?? "http://localhost:5045";

            if (string.IsNullOrEmpty(generalSettings.BaseUrl))
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
