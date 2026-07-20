using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Base;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Application.Interfaces;

namespace MeuCrudCsharp.Features.MercadoPago.Chargebacks.Infrastructure.Integration;

public class MercadoPagoChargebackIntegrationService(
    IHttpClientFactory httpClientFactory,
    ILogger<MercadoPagoChargebackIntegrationService> logger)
    : MercadoPagoServiceBase(httpClientFactory, logger),
        IMercadoPagoChargebackIntegrationService
{
    public async Task<MercadoPagoChargebackResponse?> GetChargebackDetailsFromApiAsync(
        string chargebackId
    )
    {
        var endpoint = $"v1/chargebacks/{chargebackId}";

        try
        {
            var jsonResponse = await SendMercadoPagoRequestAsync<object>(
                HttpMethod.Get,
                endpoint,
                null
            );

            if (string.IsNullOrEmpty(jsonResponse))
                return null;

            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            options.NumberHandling = JsonNumberHandling.AllowReadingFromString;

            return JsonSerializer.Deserialize<MercadoPagoChargebackResponse>(jsonResponse, options);
        }
        catch (ExternalApiException)
        {
            // O Base Service já logou o erro externo, apenas repassamos
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Falha de deserialização ou erro interno ao buscar chargeback {Id}",
                chargebackId
            );
            throw new AppServiceException("Erro ao processar resposta do Mercado Pago.", ex);
        }
    }

    public async Task UploadDocumentationAsync(string chargebackId, IFormFileCollection files)
    {
        var endpoint = $"v1/chargebacks/{chargebackId}/documentation";

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        
        // MercadoPago Base service doesn't have a direct method for multipart/form-data out of the box in SendMercadoPagoRequestAsync.
        // I will use HttpClient directly here but add the required authorization header.
        
        using var content = new MultipartFormDataContent();
        
        foreach (var file in files)
        {
            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "files", file.FileName);
        }

        request.Content = content;
        
        // Ensure to use the configured client with auth header if MercadoPagoServiceBase configures it,
        // or configure it here manually if needed. I will rely on the base class' SendMercadoPagoRequestAsync if possible,
        // but since SendMercadoPagoRequestAsync usually serializes JSON, I have to do it manually.
        
        try
        {
            var response = await HttpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao enviar documentação para chargeback {Id}", chargebackId);
            throw new AppServiceException("Erro ao processar o upload da documentação no Mercado Pago.", ex);
        }
    }
}