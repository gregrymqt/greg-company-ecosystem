using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace MeuCrudCsharp.Extensions.Services.Persistence;

public static class DistributedServicesExtensions
{
    public static WebApplicationBuilder AddDistributedServices(this WebApplicationBuilder builder)
    {
        var redisSettings = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<RedisSettings>>().Value;

        if (redisSettings.UseRedis)
        {
            if (string.IsNullOrEmpty(redisSettings.ConnectionString))
            {
                throw new InvalidOperationException(
                    "A variável USE_REDIS é true mas a connection string está vazia."
                );
            }
            builder.AddRedisPersistence(redisSettings);
        }
        else
        {
            builder.AddInMemoryPersistence();
        }

        return builder;
    }

    private static void AddRedisPersistence(
        this WebApplicationBuilder builder,
        RedisSettings redisSettings
    )
    {
        var options = ConfigurationOptions.Parse(redisSettings.ConnectionString!);

        if (!string.IsNullOrWhiteSpace(redisSettings.Password) && string.IsNullOrEmpty(options.Password))
        {
            options.Password = redisSettings.Password;
        }

        var multiplexer = ConnectionMultiplexer.Connect(options);
        builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

        builder.Services.AddStackExchangeRedisCache(cacheOptions =>
        {
            cacheOptions.ConfigurationOptions = options;
            cacheOptions.InstanceName = "MeuApp_";
        });

        builder.Services.AddHangfire(config => config.UseRedisStorage(multiplexer));
        builder.Services.AddHangfireServer();
        Console.WriteLine("--> Usando Redis para Cache Distribuído e Hangfire (Conexão Singleton Autenticada).");
    }

    private static void AddInMemoryPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddHangfire(config => config.UseInMemoryStorage());
        builder.Services.AddHangfireServer();
        Console.WriteLine("--> Usando Cache em Memória para Cache Distribuído e Hangfire.");
    }
}
