using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MeuCrudCsharp.Extensions.Services.Persistence;

public static class IdentityServicesExtensions
{
    public static WebApplicationBuilder AddIdentityPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddIdentity<Users, Roles>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<ApiDbContext>()
        .AddDefaultTokenProviders();

        return builder;
    }
}