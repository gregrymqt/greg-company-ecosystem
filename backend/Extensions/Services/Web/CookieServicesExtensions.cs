namespace MeuCrudCsharp.Extensions.Services.Web;

public static class CookieServicesExtensions
{
    public static WebApplicationBuilder AddCookiePolicies(this WebApplicationBuilder builder)
    {
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/ExternalLogin";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";

            options.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        });

        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.Lax;
            options.OnAppendCookie = cookieContext =>
            {
                if (cookieContext.CookieName.StartsWith(".AspNetCore.Correlation.") || 
                    cookieContext.CookieName.StartsWith(".AspNetCore.OpenIdConnect.Nonce."))
                {
                    cookieContext.CookieOptions.SameSite = SameSiteMode.None;
                }
            };
        });

        return builder;
    }
}