using MeuCrudCsharp.Features.Hubs;
using MeuCrudCsharp.Features.MercadoPago.Jobs;
using MeuCrudCsharp.Features.MercadoPago.Jobs.Job;
using MeuCrudCsharp.Features.MercadoPago.Jobs.Services;

namespace MeuCrudCsharp.Extensions;

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.Scan(scan =>
            scan.FromEntryAssembly()
                .AddClasses(classes =>
                    classes.InNamespaces(
                        "MeuCrudCsharp.Features.Courses.Application.Services",
                        "MeuCrudCsharp.Features.Courses.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.Support.Services",
                        "MeuCrudCsharp.Features.Support.Repositories",
                        "MeuCrudCsharp.Features.Profiles.Admin.Services",
                        "MeuCrudCsharp.Features.Profiles.Admin.Repositories",
                        "MeuCrudCsharp.Features.Auth.Application.Services",
                        "MeuCrudCsharp.Features.Auth.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.Files.Application.Services",
                        "MeuCrudCsharp.Features.Files.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.Profiles.UserAccount.Services",
                        "MeuCrudCsharp.Features.Profiles.UserAccount.Repositories",
                        "MeuCrudCsharp.Features.Profiles.Admin.Services",
                        "MeuCrudCsharp.Features.Profiles.Admin.Repositories",
                        "MeuCrudCsharp.Features.Videos.Services",
                        "MeuCrudCsharp.Features.Videos.Repositories",
                        "MeuCrudCsharp.Features.Videos.Notification",
                        "MeuCrudCsharp.Features.About.Application.Services",
                        "MeuCrudCsharp.Features.About.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.Home.Application.Services",
                        "MeuCrudCsharp.Features.Home.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.Caching.Application.Services",
                        "MeuCrudCsharp.Features.Emails.Infrastructure.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Payments.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Payments.Repositories",
                        "MeuCrudCsharp.Features.MercadoPago.Notification.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Plans.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Plans.Repositories",
                        "MeuCrudCsharp.Features.MercadoPago.Clients.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Subscriptions.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Subscriptions.Repositories",
                        "MeuCrudCsharp.Features.MercadoPago.Refunds.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Refunds.Notifications",
                        "MeuCrudCsharp.Features.MercadoPago.Jobs.Services",
                        "MeuCrudCsharp.Features.MercadoPago.WebHooks.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Chargebacks.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Chargebacks.Notifications",
                        "MeuCrudCsharp.Features.MercadoPago.Claims.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Claims.Repositories",
                        "MeuCrudCsharp.Features.MercadoPago.Hub",
                        "MeuCrudCsharp.AppSettings",
                        "MeuCrudCsharp.Features.Shared.Work"
                    )
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        builder.Services.AddScoped<ProcessPaymentNotificationJob>();

        builder.Services.Configure<GeneralSettings>(
            builder.Configuration.GetSection(GeneralSettings.SectionName)
        );

        builder.Services.Configure<MercadoPagoSettings>(
            builder.Configuration.GetSection(MercadoPagoSettings.SectionName)
        );

        builder.Services.Configure<SendGridSettings>(
            builder.Configuration.GetSection(SendGridSettings.SectionName)
        );

        builder.Services.Configure<GoogleSettings>(
            builder.Configuration.GetSection(GoogleSettings.SectionName)
        );

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

        builder.Services.AddSingleton<ConnectionMapping<string>>();

        builder.Services.Configure<FFmpegSettings>(
            builder.Configuration.GetSection("FFmpegSettings")
        );

        return builder;
    }
}
