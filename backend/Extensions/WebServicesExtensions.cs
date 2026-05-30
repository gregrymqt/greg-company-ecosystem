using MercadoPago.Config;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Polly;

namespace MeuCrudCsharp.Extensions;

public static class WebServicesExtensions
{
    public const string CorsPolicyName = "_myAllowSpecificOrigins";

    public static WebApplicationBuilder AddWebServices(this WebApplicationBuilder builder)
    {
        var mercadoPagoSettings = builder
            .Configuration.GetSection("MercadoPago")
            .Get<MercadoPagoSettings>();

        if (mercadoPagoSettings is null || string.IsNullOrEmpty(mercadoPagoSettings.AccessToken))
        {
            throw new InvalidOperationException(
                "Configurações do Mercado Pago não encontradas ou o AccessToken está vazio."
            );
        }

        builder
            .Services.AddHttpClient(
                "MercadoPagoClient",
                client =>
                {
                    client.BaseAddress = new Uri("https://api.mercadopago.com");
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "Bearer",
                            mercadoPagoSettings.AccessToken
                        );

                    client.Timeout = TimeSpan.FromSeconds(30);
                }
            )
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine(
                            $"[Mercado Pago] Tentativa {retryAttempt} de 3 após {timespan.TotalSeconds}s. Erro: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}"
                        );
                    }
                )
            )
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, duration) =>
                    {
                        Console.WriteLine(
                            $"[Mercado Pago] ⚠️ CIRCUIT BREAKER ABERTO! Pausando requisições por {duration.TotalSeconds}s devido a falhas consecutivas."
                        );
                    },
                    onReset: () =>
                    {
                        Console.WriteLine(
                            "[Mercado Pago] ✅ Circuit Breaker fechado. Requisições normalizadas."
                        );
                    }
                )
            );

        MercadoPagoConfig.AccessToken = mercadoPagoSettings.AccessToken;

        builder.Services.AddCors(options =>
        {
            var generalSettings = builder
                .Configuration.GetSection("General")
                .Get<GeneralSettings>();

            if (generalSettings is null || string.IsNullOrEmpty(generalSettings.BaseUrl))
            {
                throw new InvalidOperationException(
                    "Configurações do General não encontradas ou o BaseUrl está vazio."
                );
            }
            options.AddPolicy(
                name: CorsPolicyName,
                policy =>
                {
                    policy
                        .WithOrigins(
                            generalSettings.BaseUrl, // Exemplo para Ngrok
                            "http://localhost:5045", // Exemplo para desenvolvimento local
                            "http://localhost:5173"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            );
        });

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        builder.ConfigureCookiePolicies();

        return builder;
    }

    public static IApplicationBuilder UseGregNetworkMonitoring(this IApplicationBuilder app)
    {
        return app.Use(
            async (context, next) =>
            {
                await next();

                var logData = new
                {
                    source = "Back",
                    method = context.Request.Method,
                    url = context.Request.Path.Value,
                    status = context.Response.StatusCode,
                };

                try
                {
                    using var client = new HttpClient();
                    await client.PostAsJsonAsync("http://localhost:8888/log", logData);
                }
                catch
                {
                }
            }
        );
    }

    private static void ConfigureCookiePolicies(this WebApplicationBuilder builder)
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
                if (
                    cookieContext.CookieName.StartsWith(".AspNetCore.Correlation.")
                    || cookieContext.CookieName.StartsWith(".AspNetCore.OpenIdConnect.Nonce.")
                )
                {
                    cookieContext.CookieOptions.SameSite = SameSiteMode.None;
                }
            };
        });
    }
}
