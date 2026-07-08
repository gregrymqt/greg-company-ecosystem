using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MeuCrudCsharp.Extensions.Services.Integration;

public static class RabbitMqExtensions
{
    public static WebApplicationBuilder AddRabbitMqIntegration(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IConnection>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("RabbitMqConfiguration");
            
            var hostname = configuration["RabbitMQ:HostName"] ?? "localhost";
            var port = configuration.GetValue<int?>("RabbitMQ:Port") ?? 5672;
            
            var factory = new ConnectionFactory
            {
                HostName = hostname,
                Port = port
            };

            // Setup further configuration like Username, Password here if needed
            // factory.UserName = configuration["RabbitMQ:UserName"];
            // factory.Password = configuration["RabbitMQ:Password"];

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
