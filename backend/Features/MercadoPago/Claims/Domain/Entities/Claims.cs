using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
 
using MeuCrudCsharp.Features.Auth.Domain.Entities; 
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities
{


    public class Claims : IMongoDocument
    {
        public static string CollectionName => "claims";

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!; // ID Interno do Banco [cite: 19]

        // Este ÃƒÂ© o ID real que o MP usa (ex: "5012391221")
        [MongoIndex(Unique = true)]
        public string MercadoPagoClaimId { get; set; } = null!; // Mudado para string conforme instrução

        // ID do pagamento vinculado (Payment ID)
        public string? PaymentId { get; set; }

        // Agora usando o Enum forte em vez de string
        public ClaimType Type { get; set; } // "mediations", "cancel_purchase" etc [cite: 22]

        // Agora usando o Enum forte em vez de string
        public ClaimResource? ResourceType { get; set; } // "payment", "subscription" etc [cite: 23]

        // Novo campo sugerido para guardar o status original do MP (opened/closed) separadamente do seu status interno
        public ClaimStage? CurrentStage { get; set; } // claim, dispute, etc [cite: 4]

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; }

        public string? Resolution { get; set; }

        // Seu status interno de controle
        public ClaimStatus Status { get; set; } = ClaimStatus.Opened; 

        public string? MercadoPagoPanelUrl =>
            $"https://www.mercadopago.com.br/developers/panel/notifications/claims/{MercadoPagoClaimId}"; 

        public string? UserId { get; set; }
    
        [BsonIgnore]
        public virtual Users? User { get; set; }
    }
}



