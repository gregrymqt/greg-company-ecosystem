using Microsoft.EntityFrameworkCore;
using MeuCrudCsharp.Data;

namespace MeuCrudCsharp.Extensions.Services.Persistence;

public static class PostgresServicesExtensions
{
    public static WebApplicationBuilder AddPostgresPersistence(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        // Lê a connection string do Transaction Pooler configurada no .env
        var connectionString = configuration.GetConnectionString("PostgresTransaction");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            }));

        return builder;
    }
}
