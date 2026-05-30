using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; using MeuCrudCsharp.Features.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Models;

/// <summary>
/// Define o status interno de um chargeback para acompanhamento.
/// </summary>
public enum ChargebackStatus
{
    [Display(Name = "Novo")]
    Novo, // O chargeback acabou de ser registrado e aguarda anÃ¡lise inicial.

    [Display(Name = "Aguardando EvidÃªncias")]
    AguardandoEvidencias, // A equipe precisa coletar e enviar as evidÃªncias para a disputa.

    [Display(Name = "EvidÃªncias Enviadas")]
    EvidenciasEnviadas, // As evidÃªncias foram enviadas e aguardamos a resoluÃ§Ã£o.

    [Display(Name = "Ganhamos")]
    Ganhamos, // A disputa foi resolvida a nosso favor.

    [Display(Name = "Perdemos")]
    Perdemos, // A disputa foi resolvida a favor do cliente.
}

/// <summary>
/// Representa uma notificaÃ§Ã£o de chargeback recebida do Mercado Pago.
/// </summary>
[Index(nameof(ChargebackId), IsUnique = true)]
[Index(nameof(PaymentId))]
[Index(nameof(UserId))]
public class Chargeback
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// ID do chargeback no Mercado Pago (vem de `data.id`).
    /// </summary>
    [Required]
    public long ChargebackId { get; set; }

    /// <summary>
    /// ID do pagamento associado ao chargeback (vem de `data.payment_id`).
    /// </summary>
    [Required]
    public long PaymentId { get; set; }

    /// <summary>
    /// FK para o usuÃ¡rio que realizou o pagamento original.
    /// </summary>
    public string? UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual Users? User { get; set; }

    /// <summary>
    /// Status interno para acompanhamento da equipe.
    /// </summary>
    [Required]
    public ChargebackStatus Status { get; set; } = ChargebackStatus.Novo;

    /// <summary>
    /// Valor do chargeback. Este campo precisarÃ¡ ser preenchido consultando a API do MP.
    /// </summary>
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Data em que o registro foi criado no banco de dados.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Campo de texto para notas internas da equipe sobre o chargeback.
    /// </summary>
    public string? InternalNotes { get; set; }
}
