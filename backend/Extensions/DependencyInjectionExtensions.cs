using MeuCrudCsharp.Features.Hubs.Infrastructure.State;
using MeuCrudCsharp.Features.Hubs.Presentation.Hubs;
using MeuCrudCsharp.Features.MercadoPago.Jobs;
using MeuCrudCsharp.Features.MercadoPago.Jobs.Application.Jobs;
using MeuCrudCsharp.Features.MercadoPago.Jobs.Infrastructure.Services;

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
                        "MeuCrudCsharp.Features.Support.Application.Services",
                        "MeuCrudCsharp.Features.Support.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.Profiles.Admin.Application.Services",
                        "MeuCrudCsharp.Features.Profiles.Admin.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.Auth.Application.Services",
                        "MeuCrudCsharp.Features.Auth.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.Files.Application.Services",
                        "MeuCrudCsharp.Features.Files.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.Profiles.UserAccount.Application.Services",
                        "MeuCrudCsharp.Features.Profiles.UserAccount.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.Profiles.Admin.Application.Services",
                        "MeuCrudCsharp.Features.Profiles.Admin.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.Videos.Application.Services",
                        "MeuCrudCsharp.Features.Videos.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.Videos.Application.Notification",
                        "MeuCrudCsharp.Features.About.Application.Services",
                        "MeuCrudCsharp.Features.About.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.Home.Application.Services",
                        "MeuCrudCsharp.Features.Home.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.Caching.Application.Services",
                        "MeuCrudCsharp.Features.Emails.Infrastructure.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Payments.Application.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Payments.Infrastructure.Integration",
                        "MeuCrudCsharp.Features.MercadoPago.Payments.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.MercadoPago.Notification.Application.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Plans.Application.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Plans.Infrastructure.Integration",
                        "MeuCrudCsharp.Features.MercadoPago.Plans.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.MercadoPago.Clients.Application.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Clients.Infrastructure.Integration",
                        "MeuCrudCsharp.Features.MercadoPago.Subscriptions.Application.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Subscriptions.Infrastructure.Integration",
                        "MeuCrudCsharp.Features.MercadoPago.Subscriptions.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.MercadoPago.Refunds.Infrastructure.Integration",
                        "MeuCrudCsharp.Features.MercadoPago.Refunds.Application.Notifications",
                        "MeuCrudCsharp.Features.MercadoPago.Jobs.Infrastructure.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Webhooks.Application.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Chargebacks.Application.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Chargebacks.Infrastructure.Integration",
                        "MeuCrudCsharp.Features.MercadoPago.Chargebacks.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.MercadoPago.Claims.Application.Services",
                        "MeuCrudCsharp.Features.MercadoPago.Claims.Infrastructure.Integration",
                        "MeuCrudCsharp.Features.MercadoPago.Claims.Infrastructure.Persistence.Repositories",
                        "MeuCrudCsharp.Features.MercadoPago.Hub.Application.Interfaces",
                        "MeuCrudCsharp.Features.MercadoPago.Hub.Infrastructure.Services",
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
