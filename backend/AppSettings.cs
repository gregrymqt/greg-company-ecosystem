public class GeneralSettings
{
    public const string SectionName = "GENERAL";
    public string? BaseUrl { get; set; }
    public string? FrontendUrl { get; set; }
    public bool UseKubernetesLogs { get; set; }
    public bool UseRedis { get; set; }
}

public class MercadoPagoSettings
{
    public const string SectionName = "MercadoPago";

    public string? PublicKey { get; set; }
    public string? AccessToken { get; set; }
    public string? WebhookSecret { get; set; }
    public string? Defaultdescription { get; set; }
}

public class SendGridSettings
{
    public const string SectionName = "SendGrid";
    public string? ApiKey { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
}

public class JwtSettings
{
    public const string SectionName = "Jwt";
    public string? Key { get; set; }
}

public class GoogleSettings
{
    public const string SectionName = "Google";
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
}

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";
    public string? ConnectionString { get; set; }
    public string? HostName { get; set; }
    public int Port { get; set; } = 5672;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? VirtualHost { get; set; }
}

public class SupabaseSettings
{
    public const string SectionName = "Supabase";
    public string? AccessKeyID { get; set; }
    public string? SecretAccessKey { get; set; }
    public string? EndPointS3 { get; set; }
}

public class RedisSettings
{
    public const string SectionName = "Redis";
    public bool UseRedis { get; set; }
    public string? ConnectionString { get; set; }
    public string? Password { get; set; }
}

public class PostgresSettings
{
    public const string SectionName = "Postgres";
    public string? TransactionConnectionString { get; set; }
    public string? SessionConnectionString { get; set; }
}