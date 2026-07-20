using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MeuCrudCsharp.Extensions.Services.Integration;

public static class RabbitMqExtensions
{
    public static WebApplicationBuilder AddRabbitMqIntegration(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IConnection>(sp =>
        {
            var rabbitMqSettings = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("RabbitMqConfiguration");
            
            var hostname = rabbitMqSettings.HostName ?? "localhost";
            var port = rabbitMqSettings.Port > 0 ? rabbitMqSettings.Port : 5672;
            
            var factory = new ConnectionFactory
            {
                HostName = hostname,
                Port = port,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };

            if (!string.IsNullOrEmpty(rabbitMqSettings.UserName))
            {
                factory.UserName = rabbitMqSettings.UserName;
            }
            if (!string.IsNullOrEmpty(rabbitMqSettings.Password))
            {
                factory.Password = rabbitMqSettings.Password;
            }
            if (!string.IsNullOrEmpty(rabbitMqSettings.VirtualHost))
            {
                factory.VirtualHost = rabbitMqSettings.VirtualHost;
            }

            try
            {
                logger.LogInformation("Conectando ao RabbitMQ em {Hostname}:{Port}...", hostname, port);
                return factory.CreateConnection();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao conectar no RabbitMQ.");
                throw;
            }
        });

        builder.Services.AddSingleton<MeuCrudCsharp.Features.Shared.Application.Interfaces.IRabbitMqPublisher, MeuCrudCsharp.Features.Shared.Infrastructure.Messaging.RabbitMqPublisher>();

        return builder;
    }
}
