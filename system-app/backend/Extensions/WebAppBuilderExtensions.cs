using System.Text.Json;

namespace MeuCrudCsharp.Extensions;

public static class WebAppBuilderExtensions
{
    public static WebApplicationBuilder AddCoreServices(this WebApplicationBuilder builder)
    {
        builder
            .Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizeFolder("/", "RequireJwtToken");

            options.Conventions.AllowAnonymousToPage("/Index");
            options.Conventions.AllowAnonymousToPage("/Account/ExternalLogin");
            options.Conventions.AllowAnonymousToPage("/Account/Logout");
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSignalR();

        return builder;
    }
}
