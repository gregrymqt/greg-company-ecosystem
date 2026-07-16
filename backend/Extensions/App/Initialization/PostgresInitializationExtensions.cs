using Microsoft.EntityFrameworkCore;
using MeuCrudCsharp.Data;

namespace MeuCrudCsharp.Extensions.App.Initialization;

public static class PostgresInitializationExtensions
{
    public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Executa as Migrations pendentes no startup de forma assíncrona
        await context.Database.MigrateAsync();
    }
}
