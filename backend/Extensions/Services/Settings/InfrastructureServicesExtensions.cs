using MeuCrudCsharp.Features.Hubs.Infrastructure.State;
using MeuCrudCsharp.Features.MercadoPago.Jobs.Application.Jobs;

namespace MeuCrudCsharp.Extensions.Services;

public static class InfrastructureServicesExtensions
{
    public static WebApplicationBuilder AddInfrastructureComponents(this WebApplicationBuilder builder)
    {
        // Background Jobs
        builder.Services.AddScoped<ProcessPaymentNotificationJob>();

        // SignalR Stateful Mappings
        builder.Services.AddSingleton<ConnectionMapping<string>>();

        return builder;
    }
}