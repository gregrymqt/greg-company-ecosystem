using DotNetEnv;
using MeuCrudCsharp.Extensions.Services;
using MeuCrudCsharp.Extensions.App;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "log/log-.txt",
        rollingInterval: RollingInterval.Day,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1),
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

try
{
    Log.Information("Iniciando o ecossistema Greg Company...");
    Env.Load();

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // 1. Centralização da Injeção de Dependências (Services)
    builder.ConfigureAllServices();

    var app = builder.Build();

    // 2. Centralização do Fluxo de Middlewares (App Pipeline)
    await app.ConfigureAppPipeline();

    app.Run();
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