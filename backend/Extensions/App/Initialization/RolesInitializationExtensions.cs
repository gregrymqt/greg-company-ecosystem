using Microsoft.AspNetCore.Identity;
using MeuCrudCsharp.Features.Auth.Domain.Entities;

namespace MeuCrudCsharp.Extensions.App.Initialization;

public static class RolesInitializationExtensions
{
    public static async Task InitializeRolesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Roles>>();

        var roles = new[]
        {
            AppRoles.Admin,
            AppRoles.User,
            AppRoles.Manager,
            AppRoles.CourseAdmin,
            AppRoles.EcommerceAdmin
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new Roles(role));
                Console.WriteLine($"--> [Roles] Role '{role}' garantida no banco de dados.");
            }
        }
    }
}
