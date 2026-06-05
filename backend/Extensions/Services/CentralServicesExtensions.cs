using MeuCrudCsharp.Extensions.Services.Integration; // Novo
using MeuCrudCsharp.Extensions.Services.Mcp;
using MeuCrudCsharp.Extensions.Services.Persistence;
using MeuCrudCsharp.Extensions.Services.Presentation;
using MeuCrudCsharp.Extensions.Services.Web; // Novo

namespace MeuCrudCsharp.Extensions.Services;

public static class CentralServicesExtensions
{
    public static WebApplicationBuilder ConfigureAllServices(this WebApplicationBuilder builder)
    {
        builder
            .AddAppSettingsOptions()
            .AddFeatureDiscovery()
            .AddApiServices()
            .AddRazorServices()
            .AddSwaggerServices()
            .AddSignalRServices()
            .AddInfrastructureComponents()
            .AddSqlPersistence()
            .AddIdentityPersistence()
            .AddMongoPersistence()
            .AddDistributedServices()
            // --- OS 3 NOVOS MÓDULOS DE WEB E INTEGRAÇÃO ---
            .AddMercadoPagoIntegration()
            .AddWebSecurity()
            .AddCookiePolicies()
            .AddAuthConfiguration();

        builder.Services.AddMcpContextServer(builder.Configuration);

        return builder;
    }
}
