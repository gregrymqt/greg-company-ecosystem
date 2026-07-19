using DotNetEnv;
using MeuCrudCsharp.Extensions.Services;
using MeuCrudCsharp.Extensions.App;
using Serilog;

var useK8sLogs = Environment.GetEnvironmentVariable("USE_KUBERNETES_LOGS") == "true";

var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console();

if (!useK8sLogs)
{
    loggerConfig.WriteTo.File(
        "log/log-.txt",
        rollingInterval: RollingInterval.Day,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1),
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    );
}

Log.Logger = loggerConfig.CreateLogger();

try
{
    Log.Information("Iniciando o ecossistema Greg Company...");

    // 💡 Procura o .env localmente ou sobe um nível (Monorepo-friendly)
    if (File.Exists(".env"))
    {
        Env.Load(".env");
        Log.Information("--> .env carregado a partir da raiz de execução.");
    }
    else if (File.Exists("../.env"))
    {
        Env.Load("../.env");
        Log.Information("--> .env carregado a partir do diretório superior (..).");
    }
    else
    {
        Log.Warning("--> [Aviso] Nenhum arquivo .env foi encontrado. Usando variáveis de ambiente nativas.");
    }

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // 1. Centralização da Injeção de Dependências (Services)
    builder.ConfigureAllServices();

    var app = builder.Build();

    // 2. Centralização do Fluxo de Middlewares (App Pipeline)
    await app.ConfigureAppPipeline();

    app.Run();
}
catch (HostAbortedException)
{
    // Ignore: Normal behavior when running Entity Framework CLI tools (dotnet ef)
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação falhou criticamente ao iniciar: {ExceptionMessage}", ex.Message);
    Console.WriteLine($"FATAL EXCEPTION: {ex}");
    Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
}
finally
{
    Log.CloseAndFlush();
}