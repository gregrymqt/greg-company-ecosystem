using Hangfire;
using Hangfire.Redis.StackExchange;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace MeuCrudCsharp.Extensions;

public static class PersistenceExtensions
{
    public static WebApplicationBuilder AddPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContextFactory<ApiDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        );

        builder
            .Services.AddIdentity<Users, Roles>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;

                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<ApiDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddSingleton<IMongoClient>(_ =>
        {
            var mongoConnString = builder.Configuration.GetConnectionString("MongoConnection");
            return new MongoClient(mongoConnString);
        });

        builder.Services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase("MeuCrudSupportDb");
        });

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