using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Dtos;

public record CreatePixPaymentRequest(
    string? Description,
    decimal TransactionAmount,
    PayerRequestDto? Payer
);

public record CreditCardPaymentRequestDto(
    [Required] string Token,
    [Required] int Installments,
    [Required] string PaymentMethodId,
    [Required] string IssuerId,
    [Required] decimal Amount,
    string? Plano,
    string? PlanExternalId,
    [Required] PayerRequestDto Payer
);

public record IdentificationDto(string? Type, string? Number);

public record MercadoPagoPaymentDetails(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("external_reference")] string ExternalReference,
    [property: JsonPropertyName("payer")] PayerRequestDto Payer
);

public record PayerRequestDto(
    string? Email,
    string? FirstName,
    string? LastName,
    IdentificationDto? Identification
);

public record PaymentResponseDto(
    string? Status,
    long? Id,
    string? PaymentTypeId,
    string? Message,
    string? QrCodeBase64,
    string? QrCode
);

public record CreatePreferenceDto(
    decimal Amount,
    string Title,
    string Description
);

public class PaymentHistoryDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("paymentMethod")]
    public string? PaymentMethod { get; set; }
}
