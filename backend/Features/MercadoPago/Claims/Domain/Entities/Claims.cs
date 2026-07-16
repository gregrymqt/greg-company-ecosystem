using MeuCrudCsharp.Features.Auth.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities
{
    public class Claim
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string MercadoPagoClaimId { get; set; } = null!;

        public string? PaymentId { get; set; }

        public ClaimType Type { get; set; }

        public ClaimResource? ResourceType { get; set; }

        public ClaimStage? CurrentStage { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; }

        public string? Resolution { get; set; }

        public ClaimStatus Status { get; set; } = ClaimStatus.Opened;

        public string? MercadoPagoPanelUrl =>
            $"https://www.mercadopago.com.br/developers/panel/notifications/claims/{MercadoPagoClaimId}";

        public Guid? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual Users? User { get; set; }
    }
}
