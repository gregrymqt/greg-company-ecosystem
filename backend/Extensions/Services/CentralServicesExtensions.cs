using System;
using MeuCrudCsharp.Extensions.Services.Integration; // Novo
using MeuCrudCsharp.Extensions.Services.Mcp;
using MeuCrudCsharp.Extensions.Services.Persistence;
using MeuCrudCsharp.Extensions.Services.Presentation;
using MeuCrudCsharp.Extensions.Services.Web; // Novo
using MeuCrudCsharp.Features.Shared.Infrastructure.Workers;
using MeuCrudCsharp.Features.Videos.Shared.Infrastructure.Workers;

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
            .AddPostgresPersistence()
            .AddDistributedServices()
            .AddMercadoPagoIntegration()
            .AddRabbitMqIntegration()
            .AddWebSecurity()
            .AddCookiePolicies()
            .AddAuthConfiguration();

        builder.Services.AddMcpContextServer();
        builder.Services.AddHostedService<OutboxProcessorWorker>();
        builder.Services.AddHostedService<VideoProcessingCompletedConsumer>();
        builder.Services.AddHealthChecks()
            .AddNpgSql(
                connectionStringFactory: sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<PostgresSettings>>().Value.TransactionConnectionString!,
                name: "supabase_postgresql",
                timeout: TimeSpan.FromSeconds(3)
            );

        return builder;
    }
}
