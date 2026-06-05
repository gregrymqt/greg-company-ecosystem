using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MeuCrudCsharp.Features.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace MeuCrudCsharp.Extensions.Services;

public static class AuthServicesExtensions
{
    public static WebApplicationBuilder AddAuthConfiguration(this WebApplicationBuilder builder)
    {
        // Limpa mapeamentos padrão de Claims e força a cultura global
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        ConfigureGlobalCulture();

        // Configuração de Esquemas de Autenticação
        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddGoogleProvider(builder.Configuration)
            .AddJwtBearerProvider(builder.Configuration);

        // Registro isolado das Políticas de Autorização
        builder.AddAuthorizationPolicies();

        return builder;
    }

    private static void ConfigureGlobalCulture()
    {
        var cultureInfo = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }

    private static AuthenticationBuilder AddGoogleProvider(
        this AuthenticationBuilder authBuilder,
        IConfiguration configuration
    )
    {
        var googleSettings = configuration.GetSection("Google").Get<GoogleSettings>();

        if (
            googleSettings is null
            || string.IsNullOrEmpty(googleSettings.ClientId)
            || string.IsNullOrEmpty(googleSettings.ClientSecret)
        )
        {
            throw new InvalidOperationException(
                "Credenciais do Google não encontradas no arquivo de configuração."
            );
        }

        return authBuilder.AddGoogle(options =>
        {
            options.ClientId = googleSettings.ClientId;
            options.ClientSecret = googleSettings.ClientSecret;
            options.SignInScheme = IdentityConstants.ExternalScheme;
        });
    }

    private static AuthenticationBuilder AddJwtBearerProvider(
        this AuthenticationBuilder authBuilder,
        IConfiguration configuration
    )
    {
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();

        if (jwtSettings?.Key is null)
            throw new InvalidOperationException(
                "Chave de assinatura do JWT não encontrada no arquivo de configuração."
            );

        return authBuilder.AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Key)
                ),
                ValidateIssuer = false,
                ValidateAudience = false,
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero,
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.TryGetValue("jwt", out var tokenFromCookie))
                    {
                        context.Token = tokenFromCookie;
                    }
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"Token inválido interceptado: {context.Exception.Message}");
                    return Task.CompletedTask;
                },
            };
        });
    }

    private static void AddAuthorizationPolicies(this WebApplicationBuilder builder)
    {
        builder
            .Services.AddAuthorizationBuilder()
            .AddPolicy(
                "RequireJwtToken",
                policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                }
            )
            .AddPolicy(
                "ActiveSubscription",
                policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.AddRequirements(new ActiveSubscriptionRequirement());
                }
            );
    }
}
