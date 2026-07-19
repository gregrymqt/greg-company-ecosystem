using System;
using MeuCrudCsharp.Extensions.Services.Integration; // Novo
using MeuCrudCsharp.Extensions.Services.Mcp;
using MeuCrudCsharp.Extensions.Services.Persistence;
using MeuCrudCsharp.Extensions.Services.Presentation;
using MeuCrudCsharp.Extensions.Services.Web; // Novo
using MeuCrudCsharp.Features.Shared.Infrastructure.Workers;
using MeuCrudCsharp.Features.Videos.Shared.Infrastructure.Workers;
using MeuCrudCsharp.Features.Products.Shared.Infrastructure.Workers;

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
        builder.Services.AddHostedService<VideoProcessingCompletedConsumer>();
        builder.Services.AddHostedService<ProductImportCompletedConsumer>();
        builder.Services.AddHealthChecks()
            .AddNpgSql(
                connectionString: builder.Configuration.GetConnectionString("PostgresTransaction") ?? builder.Configuration["ConnectionStrings:PostgresTransaction"] ?? "",
                name: "supabase_postgresql",
                timeout: TimeSpan.FromSeconds(3)
            );

        return builder;
    }
}
