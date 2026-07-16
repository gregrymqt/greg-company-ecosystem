using Microsoft.EntityFrameworkCore;
using MeuCrudCsharp.Data;

namespace MeuCrudCsharp.Extensions.Services.Persistence;

public static class PostgresServicesExtensions
{
    public static IServiceCollection AddPostgresPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // Lê a connection string do Transaction Pooler configurada no .env
        var connectionString = configuration.GetConnectionString("PostgresTransaction");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            }));

        return services;
    }
}
