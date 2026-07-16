using MeuCrudCsharp.Extensions.Services.Integration; // Novo
using MeuCrudCsharp.Extensions.Services.Mcp;
using MeuCrudCsharp.Extensions.Services.Persistence;
using MeuCrudCsharp.Extensions.Services.Presentation;
using MeuCrudCsharp.Extensions.Services.Web; // Novo
using MeuCrudCsharp.Features.Shared.Infrastructure.Workers;

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
            .AddIdentityPersistence()
            .AddPostgresPersistence(builder.Configuration)
            .AddDistributedServices()
            .AddMercadoPagoIntegration()
            .AddRabbitMqIntegration()
            .AddWebSecurity()
            .AddCookiePolicies()
            .AddAuthConfiguration();

        builder.Services.AddMcpContextServer(builder.Configuration);
        builder.Services.AddHostedService<OutboxProcessorWorker>();
        builder.Services.AddHealthChecks();

        return builder;
    }
}
