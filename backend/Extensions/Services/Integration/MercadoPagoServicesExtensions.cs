using MercadoPago.Config;
using Polly;

namespace MeuCrudCsharp.Extensions.Services.Integration;

public static class MercadoPagoServicesExtensions
{
    public static WebApplicationBuilder AddMercadoPagoIntegration(
        this WebApplicationBuilder builder
    )
    {
        // Certifique-se de importar o namespace do MercadoPagoSettings (ex: MeuCrudCsharp.AppSettings)
        var mercadoPagoSettings = builder
            .Configuration.GetSection("MercadoPago")
            .Get<MercadoPagoSettings>();

        if (mercadoPagoSettings is null || string.IsNullOrEmpty(mercadoPagoSettings.AccessToken))
        {
            throw new InvalidOperationException(
                "Configurações do Mercado Pago não encontradas ou o AccessToken está vazio."
            );
        }

        builder
            .Services.AddHttpClient(
                "MercadoPagoClient",
                client =>
                {
                    client.BaseAddress = new Uri("https://api.mercadopago.com");
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "Bearer",
                            mercadoPagoSettings.AccessToken
                        );
                    client.Timeout = TimeSpan.FromSeconds(30);
                }
            )
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine(
                            $"[Mercado Pago] Tentativa {retryAttempt} de 3 após {timespan.TotalSeconds}s. Erro: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}"
                        );
                    }
                )
            )
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, duration) =>
                    {
                        Console.WriteLine(
                            $"[Mercado Pago] ⚠️ CIRCUIT BREAKER ABERTO! Pausando requisições por {duration.TotalSeconds}s devido a falhas consecutivas."
                        );
                    },
                    onReset: () =>
                    {
                        Console.WriteLine(
                            "[Mercado Pago] ✅ Circuit Breaker fechado. Requisições normalizadas."
                        );
                    }
                )
            );

        MercadoPagoConfig.AccessToken = mercadoPagoSettings.AccessToken;

        return builder;
    }
}
