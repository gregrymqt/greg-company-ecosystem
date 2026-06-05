using MeuCrudCsharp.Features.Auth.Application.Interfaces;
using MeuCrudCsharp.Features.Auth.Utils;
using MeuCrudCsharp.Features.Hubs.Infrastructure.State;
using MeuCrudCsharp.Features.MercadoPago.Jobs.Application.Jobs;

namespace MeuCrudCsharp.Extensions.Services;

public static class InfrastructureServicesExtensions
{
    public static WebApplicationBuilder AddInfrastructureComponents(
        this WebApplicationBuilder builder
    )
    {
        // UserContext — registrado manualmente pois o nome termina em 'Context'
        // e não é capturado pelas regras de nomenclatura do Scrutor.
        builder.Services.AddScoped<IUserContext, UserContext>();

        // Background Jobs (Hangfire) — registrados manualmente pois os nomes terminam
        // em 'Job' e não satisfazem as convenções do FeatureDiscoveryExtensions.
        builder.Services.AddScoped<ProcessPaymentNotificationJob>();
        builder.Services.AddScoped<ProcessCardUpdateJob>();
        builder.Services.AddScoped<ProcessChargebackJob>();
        builder.Services.AddScoped<ProcessClaimJob>();
        builder.Services.AddScoped<ProcessCreateSubscriptionJob>();
        builder.Services.AddScoped<ProcessPlanSubscriptionJob>();
        builder.Services.AddScoped<ProcessRenewalSubscriptionJob>();

        // SignalR Stateful Mappings — Singleton pois é um mapa de conexões em memória.
        builder.Services.AddSingleton<ConnectionMapping<string>>();

        return builder;
    }
}
