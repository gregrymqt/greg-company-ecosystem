using MeuCrudCsharp.Data;
using MeuCrudCsharp.Models;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Extensions.App.Initialization;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeSqlDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        await ApplyMigrationsAsync(scope.ServiceProvider);
        await SeedInitialRolesAsync(scope.ServiceProvider);
    }

    private static async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        try
        {
            var dbContext = services.GetRequiredService<ApiDbContext>();
            if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
            {
                await dbContext.Database.MigrateAsync();
                Console.WriteLine("--> [EF Core] Migrations aplicadas com sucesso.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> [EF Core] Erro ao aplicar migrations: {ex.Message}");
        }
    }

    private static async Task SeedInitialRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<Roles>>();
        string[] roles = [AppRoles.Admin, AppRoles.User, AppRoles.Manager];

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Roles(roleName));
                Console.WriteLine($"--> [Identity] Role '{roleName}' criada com sucesso.");
            }
        }
    }
}