using MeuCrudCsharp.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Extensions.Services.Persistence;

public static class PostgreServicesExtensions
{
    public static WebApplicationBuilder AddPostgrePersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApiDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        );

        return builder;
    }
}
