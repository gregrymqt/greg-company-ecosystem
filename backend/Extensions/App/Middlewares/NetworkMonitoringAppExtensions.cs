using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace MeuCrudCsharp.Extensions.App.Middlewares;

public static class NetworkMonitoringAppExtensions
{
    public static IApplicationBuilder UseGregNetworkMonitoring(this IApplicationBuilder app)
    {
        return app.Use(
            async (context, next) =>
            {
                await next();

                var logData = new
                {
                    source = "Back",
                    method = context.Request.Method,
                    url = context.Request.Path.Value,
                    status = context.Response.StatusCode,
                };

                try
                {
                    var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                    using var client = httpClientFactory.CreateClient();
                    await client.PostAsJsonAsync("http://localhost:8888/log", logData);
                }
                catch
                {
                    // Silencia erros caso o endpoint de monitoramento de rede não esteja no ar
                }
            }
        );
    }
}
