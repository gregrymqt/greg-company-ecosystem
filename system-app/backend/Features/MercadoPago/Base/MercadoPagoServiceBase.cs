using System.Text;
using System.Text.Json;
using MeuCrudCsharp.Features.Exceptions;

namespace MeuCrudCsharp.Features.MercadoPago.Base
{
    public abstract class MercadoPagoServiceBase
    {
        protected readonly ILogger Logger;
        protected readonly HttpClient HttpClient;

        protected MercadoPagoServiceBase(IHttpClientFactory httpClientFactory, ILogger logger)
        {
            HttpClient =
                httpClientFactory.CreateClient("MercadoPagoClient")
                ?? throw new ArgumentNullException(
                    nameof(httpClientFactory),
                    "HttpClient não pode ser nulo."
                );
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected async Task<string> SendMercadoPagoRequestAsync<T>(
            HttpMethod method,
            string endpoint,
            T? payload
        )
        {
            ArgumentNullException.ThrowIfNull(method);

            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Endpoint não pode ser vazio.", nameof(endpoint));

            if (HttpClient.BaseAddress == null)
                throw new InvalidOperationException("HttpClient BaseAddress não está configurado.");

            var requestUri = new Uri(HttpClient.BaseAddress, endpoint);
            var request = new HttpRequestMessage(method, requestUri);

            if (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch)
            {
                var idempotencyKey = Guid.NewGuid().ToString();
                request.Headers.Add("X-Idempotency-Key", idempotencyKey);
            }

            if (payload != null)
            {
                var jsonPayload = JsonSerializer.Serialize(payload);
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                Logger.LogInformation(
                    "Enviando requisição para MP. Método: {Method}, Endpoint: {Endpoint}",
                    method.Method,
                    endpoint
                );

                Logger.LogDebug("Payload: {Payload}", jsonPayload);
            }

            try
            {
                var response = await HttpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Logger.LogError(
                    "Erro na API do Mercado Pago. Status: {StatusCode}. Resposta: {ErrorContent}",
                    response.StatusCode,
                    errorContent
                );

                var errorMessage =
                    $"Erro na API do Mercado Pago. Status: {response.StatusCode}. Detalhes: {errorContent}";
                throw new ExternalApiException(
                    errorMessage,
                    new HttpRequestException(errorMessage, null, response.StatusCode)
                );
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(
                    ex,
                    "Erro de comunicação com o provedor de pagamentos. Status: {StatusCode}",
                    ex.StatusCode
                );
                throw new ExternalApiException(
                    "Erro de comunicação com o provedor de pagamentos.",
                    ex
                );
            }
            catch (ExternalApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro inesperado ao enviar requisição para o Mercado Pago.");
                throw new AppServiceException("Ocorreu um erro inesperado em nosso sistema.", ex);
            }
        }
    }
}
