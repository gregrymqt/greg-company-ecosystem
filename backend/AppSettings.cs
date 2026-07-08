public class GeneralSettings
{
    public const string SectionName = "GENERAL";
    public string? BaseUrl { get; set; }
    public string? FrontendUrl { get; set; }
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

public class FFmpegSettings
{
    public const string SectionName = "FFmpeg";
    public string? FfmpegPath { get; set; }
    public string? FfprobePath { get; set; }
}

public class MongoDbSettings
{
    public const string SectionName = "MongoDb";
    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
    public string? WriteConcern { get; set; }
    public bool Journal { get; set; }
}

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";
    public string? HostName { get; set; }
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