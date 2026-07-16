using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MeuCrudCsharp.Data;
using Microsoft.AspNetCore.Identity;

namespace MeuCrudCsharp.Extensions.Services.Persistence;

public static class IdentityServicesExtensions
{
    public static WebApplicationBuilder AddIdentityPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddIdentity<Users, Roles>(identityOptions =>
        {
            identityOptions.SignIn.RequireConfirmedAccount = true;
            identityOptions.Password.RequireDigit = false;
            identityOptions.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        return builder;
    }
}
