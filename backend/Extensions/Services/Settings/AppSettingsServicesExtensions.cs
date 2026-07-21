using Microsoft.Extensions.Options;

namespace MeuCrudCsharp.Extensions.Services;

public static class AppSettingsServicesExtensions
{
    public static WebApplicationBuilder AddAppSettingsOptions(this WebApplicationBuilder builder)
    {
        // 1. GeneralSettings
        builder.Services.Configure<GeneralSettings>(options =>
        {
            builder.Configuration.GetSection(GeneralSettings.SectionName).Bind(options);

            options.BaseUrl ??= builder.Configuration["GENERAL__BASEURL"] ?? builder.Configuration["General:BaseUrl"] ?? "http://localhost:5045";
            options.FrontendUrl ??= builder.Configuration["VITE_GENERAL__BASEURL"] ?? builder.Configuration["General:FrontendUrl"] ?? "http://localhost:5173";

            var useK8sLogsStr = builder.Configuration["USE_KUBERNETES_LOGS"] ?? Environment.GetEnvironmentVariable("USE_KUBERNETES_LOGS");
            if (bool.TryParse(useK8sLogsStr, out var useK8sLogs))
            {
                options.UseKubernetesLogs = useK8sLogs;
            }

            var useRedisStr = builder.Configuration["USE_REDIS"] ?? Environment.GetEnvironmentVariable("USE_REDIS");
            if (bool.TryParse(useRedisStr, out var useRedis))
            {
                options.UseRedis = useRedis;
            }
        });
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<GeneralSettings>>().Value);

        // 2. MercadoPagoSettings
        builder.Services.Configure<MercadoPagoSettings>(options =>
        {
            builder.Configuration.GetSection(MercadoPagoSettings.SectionName).Bind(options);

            options.AccessToken ??= builder.Configuration["MercadoPago__AccessToken"] ?? builder.Configuration["MercadoPago:AccessToken"];
            options.PublicKey ??= builder.Configuration["MercadoPago__PublicKey"] ?? builder.Configuration["MercadoPago:PublicKey"];
            options.WebhookSecret ??= builder.Configuration["MercadoPago__WebhookSecret"] ?? builder.Configuration["MercadoPago:WebhookSecret"];
        });
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<MercadoPagoSettings>>().Value);

        // 3. SendGridSettings
        builder.Services.Configure<SendGridSettings>(options =>
        {
            builder.Configuration.GetSection(SendGridSettings.SectionName).Bind(options);

            options.ApiKey ??= builder.Configuration["SendGrid__ApiKey"] ?? builder.Configuration["SendGrid:ApiKey"];
            options.FromEmail ??= builder.Configuration["SendGrid__FromEmail"] ?? builder.Configuration["SendGrid:FromEmail"];
            options.FromName ??= builder.Configuration["SendGrid__FromName"] ?? builder.Configuration["SendGrid:FromName"];
        });
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<SendGridSettings>>().Value);

        // 4. GoogleSettings
        builder.Services.Configure<GoogleSettings>(options =>
        {
            builder.Configuration.GetSection(GoogleSettings.SectionName).Bind(options);

            options.ClientId ??= builder.Configuration["Google__ClientId"] ?? builder.Configuration["Google:ClientId"];
            options.ClientSecret ??= builder.Configuration["Google__ClientSecret"] ?? builder.Configuration["Google:ClientSecret"];
        });
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<GoogleSettings>>().Value);

        // 5. JwtSettings
        builder.Services.Configure<JwtSettings>(options =>
        {
            builder.Configuration.GetSection(JwtSettings.SectionName).Bind(options);

            options.Key ??= builder.Configuration["Jwt__Key"] ?? builder.Configuration["Jwt:Key"];
        });
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);

        // 6. RabbitMqSettings
        builder.Services.Configure<RabbitMqSettings>(options =>
        {
            builder.Configuration.GetSection(RabbitMqSettings.SectionName).Bind(options);

            options.ConnectionString ??= builder.Configuration.GetConnectionString("RabbitMq")
                ?? builder.Configuration["ConnectionStrings__RabbitMq"]
                ?? builder.Configuration["ConnectionStrings:RabbitMq"]
                ?? builder.Configuration["RABBITMQ_URL"]
                ?? Environment.GetEnvironmentVariable("RABBITMQ_URL");

            options.HostName ??= builder.Configuration["RabbitMQ__HostName"] ?? builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
            var portStr = builder.Configuration["RabbitMQ__Port"] ?? builder.Configuration["RabbitMQ:Port"];
            if (int.TryParse(portStr, out var port))
            {
                options.Port = port;
            }
            options.UserName ??= builder.Configuration["RabbitMQ__UserName"] ?? builder.Configuration["RabbitMQ:UserName"];
            options.Password ??= builder.Configuration["RabbitMQ__Password"] ?? builder.Configuration["RabbitMQ:Password"];
        });
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value);

        // 7. SupabaseSettings
        builder.Services.Configure<SupabaseSettings>(options =>
        {
            builder.Configuration.GetSection(SupabaseSettings.SectionName).Bind(options);

            options.AccessKeyID ??= builder.Configuration["Access_key_ID"] ?? Environment.GetEnvironmentVariable("Access_key_ID");
            options.SecretAccessKey ??= builder.Configuration["Secret_Access_key"] ?? Environment.GetEnvironmentVariable("Secret_Access_key");
            options.EndPointS3 ??= builder.Configuration["EndPoint_S3"] ?? Environment.GetEnvironmentVariable("EndPoint_S3");
        });
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<SupabaseSettings>>().Value);

        // 8. RedisSettings
        builder.Services.Configure<RedisSettings>(options =>
        {
            builder.Configuration.GetSection(RedisSettings.SectionName).Bind(options);

            var useRedisStr = builder.Configuration["USE_REDIS"] ?? Environment.GetEnvironmentVariable("USE_REDIS");
            if (bool.TryParse(useRedisStr, out var useRedis))
            {
                options.UseRedis = useRedis;
            }

            options.ConnectionString ??= builder.Configuration.GetConnectionString("Redis")
                ?? builder.Configuration["ConnectionStrings__Redis"]
                ?? builder.Configuration["ConnectionStrings:Redis"]
                ?? builder.Configuration["REDIS_URL"]
                ?? "redis:6379";

            options.Password ??= builder.Configuration["REDIS_PASSWORD"]
                ?? Environment.GetEnvironmentVariable("REDIS_PASSWORD");
        });
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<RedisSettings>>().Value);

        // 9. PostgresSettings
        builder.Services.Configure<PostgresSettings>(options =>
        {
            builder.Configuration.GetSection(PostgresSettings.SectionName).Bind(options);

            options.TransactionConnectionString ??= builder.Configuration.GetConnectionString("PostgresTransaction")
                ?? builder.Configuration["POSTGRES_TRANSACTION_CONNECTION_STRING"]
                ?? builder.Configuration["ConnectionStrings__PostgresTransaction"]
                ?? Environment.GetEnvironmentVariable("POSTGRES_TRANSACTION_CONNECTION_STRING");

            options.SessionConnectionString ??= builder.Configuration.GetConnectionString("PostgresSession")
                ?? builder.Configuration["POSTGRES_SESSION_CONNECTION_STRING"]
                ?? builder.Configuration["ConnectionStrings__PostgresSession"]
                ?? Environment.GetEnvironmentVariable("POSTGRES_SESSION_CONNECTION_STRING");
        });
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<PostgresSettings>>().Value);

        return builder;
    }
}
