namespace MeuCrudCsharp.Extensions.Services;

public static class AppSettingsServicesExtensions
{
    public static WebApplicationBuilder AddAppSettingsOptions(this WebApplicationBuilder builder)
    {
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

        builder.Services.Configure<JwtSettings>(
            builder.Configuration.GetSection(JwtSettings.SectionName)
        );

        builder.Services.Configure<RabbitMqSettings>(
            builder.Configuration.GetSection(RabbitMqSettings.SectionName)
        );

        builder.Services.Configure<SupabaseSettings>(
            builder.Configuration.GetSection(SupabaseSettings.SectionName)
        );

        return builder;
    }
}
