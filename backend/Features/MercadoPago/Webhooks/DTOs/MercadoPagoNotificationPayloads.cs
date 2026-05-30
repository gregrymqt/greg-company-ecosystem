using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MeuCrudCsharp.Features.MercadoPago.Webhooks.DTOs
{
    public class MercadoPagoWebhookNotification
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("date_created")]
        public DateTime DateCreated { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }

        [JsonPropertyName("data")]
        public MercadoPagoWebhookData? Data { get; set; }
    }

    public class MercadoPagoWebhookData
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    public class ChargebackNotificationPayload
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    public class PaymentNotificationData
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    public class ClaimNotificationPayload
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    public class CardUpdateNotificationPayload
    {
        [JsonPropertyName("id")]
        public string? NewCardId { get; set; }

        [JsonPropertyName("customer_id")]
        public string? CustomerId { get; set; }
    }
}
