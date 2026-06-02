namespace MeuCrudCsharp.Extensions.Services.Presentation;

public static class RazorServicesExtensions
{
    public static WebApplicationBuilder AddRazorServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages(options =>
        {
            // Protege a raiz por padrão
            options.Conventions.AuthorizeFolder("/", "RequireJwtToken");

            // Libera páginas específicas de acesso público
            options.Conventions.AllowAnonymousToPage("/Index");
            options.Conventions.AllowAnonymousToPage("/Account/ExternalLogin");
            options.Conventions.AllowAnonymousToPage("/Account/Logout");
        });

        return builder;
    }
}