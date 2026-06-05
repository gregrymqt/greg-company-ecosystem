using Hangfire;
using Hangfire.Redis.StackExchange;

namespace MeuCrudCsharp.Extensions.Services.Persistence;

public static class DistributedServicesExtensions
{
    public static WebApplicationBuilder AddDistributedServices(this WebApplicationBuilder builder)
    {
        var useRedis = builder.Configuration.GetValue<bool>("USE_REDIS");
        var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

        if (useRedis)
        {
            if (string.IsNullOrEmpty(redisConnectionString))
            {
                throw new InvalidOperationException(
                    "A variável USE_REDIS é true mas a connection string está vazia."
                );
            }
            builder.AddRedisPersistence(redisConnectionString);
        }
        else
        {
            builder.AddInMemoryPersistence();
        }

        return builder;
    }

    private static void AddRedisPersistence(
        this WebApplicationBuilder builder,
        string redisConnectionString
    )
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "MeuApp_";
        });

        builder.Services.AddHangfire(config => config.UseRedisStorage(redisConnectionString));
        builder.Services.AddHangfireServer();
        Console.WriteLine("--> Usando Redis para Cache Distribuído e Hangfire.");
    }

    private static void AddInMemoryPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddHangfire(config => config.UseInMemoryStorage());
        builder.Services.AddHangfireServer();
        Console.WriteLine("--> Usando Cache em Memória para Cache Distribuído e Hangfire.");
    }
}
