using MeuCrudCsharp.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Extensions.Services.Persistence;

public static class SqlServicesExtensions
{
    public static WebApplicationBuilder AddSqlPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContextFactory<ApiDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        );

        return builder;
    }
}
