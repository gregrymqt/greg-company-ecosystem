using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MeuCrudCsharp.Data;

namespace MeuCrudCsharp.Extensions.Services.Persistence;

public static class PostgresServicesExtensions
{
    public static WebApplicationBuilder AddPostgresPersistence(this WebApplicationBuilder builder)
    {
        var postgresSettings = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<PostgresSettings>>().Value;
        var connectionString = postgresSettings.TransactionConnectionString;

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
