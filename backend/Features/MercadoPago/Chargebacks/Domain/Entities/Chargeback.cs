using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;
using System;

namespace MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;

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
public class Chargeback : IMongoDocument
{
    public static string CollectionName => "chargebacks";

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    /// <summary>
    /// ID do chargeback no Mercado Pago.
    /// </summary>
    public string MercadoPagoChargebackId { get; set; } = string.Empty;

    /// <summary>
    /// ID do pagamento associado ao chargeback.
    /// </summary>
    public string PaymentId { get; set; } = string.Empty;

    /// <summary>
    /// FK para o usuário que realizou o pagamento original.
    /// </summary>
    public string? UserId { get; set; }

    [BsonIgnore]
    public virtual Users? User { get; set; }

    /// <summary>
    /// Status do chargeback no MercadoPago (ex: 'in_process', 'settled', 'reimbursed').
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Valor do chargeback. Este campo precisará ser preenchido consultando a API do MP.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Indica se o chargeback é elegível para o Programa de Proteção ao Vendedor.
    /// </summary>
    public bool CoverageEligible { get; set; }

    /// <summary>
    /// Indica se é necessário o envio de documentação.
    /// </summary>
    public bool DocumentationRequired { get; set; }

    /// <summary>
    /// Prazo limite estipulado pelo Mercado Pago para o envio das evidências.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Data de abertura do chargeback no MercadoPago.
    /// </summary>
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// Data em que o registro foi criado no banco de dados.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Campo de texto para notas internas da equipe sobre o chargeback.
    /// </summary>
    public string? InternalNotes { get; set; }
}

