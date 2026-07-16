using MeuCrudCsharp.Features.Auth.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;

public enum ChargebackStatus
{
    Novo,
    AguardandoEvidencias,
    EvidenciasEnviadas,
    Ganhamos,
    Perdemos,
}

public class Chargeback
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string MercadoPagoChargebackId { get; set; } = string.Empty;

    public string PaymentId { get; set; } = string.Empty;

    public Guid? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual Users? User { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public bool CoverageEligible { get; set; }

    public bool DocumentationRequired { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? InternalNotes { get; set; }
}
