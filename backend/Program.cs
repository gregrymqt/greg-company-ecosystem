using DotNetEnv;
using MeuCrudCsharp.Extensions;
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
    Log.Information("Iniciando a aplicação...");
    Env.Load();

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.AddCoreServices().AddApplicationServices().AddPersistence().AddWebServices().AddAuth();

    var app = builder.Build();

    await app.UseAppPipeline();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação falhou ao iniciar: {ExceptionMessage}", ex.Message);
    Console.WriteLine($"FATAL EXCEPTION: {ex}");
    Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
}
finally
{
    Log.CloseAndFlush();
}
