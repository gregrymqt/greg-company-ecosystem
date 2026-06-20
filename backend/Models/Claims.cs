using MeuCrudCsharp.Models.Enums; 
using MeuCrudCsharp.Features.Auth.Domain.Entities; 
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;
using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;

namespace MeuCrudCsharp.Models
{
    // Seu status interno (Mantido conforme )
    public enum InternalClaimStatus
    {
        [Display(Name = "Novo")]
        Novo,

        [Display(Name = "Em AnÃƒÂ¡lise")]
        EmAnalise,

        [Display(Name = "Respondido pelo Vendedor")]
        RespondidoPeloVendedor,

        [Display(Name = "Resolvido - Ganhamos")]
        ResolvidoGanhamos,

        [Display(Name = "Resolvido - Perdemos")]
        ResolvidoPerdemos
    }

    public class Claims : IMongoDocument
    {
        public static string CollectionName => "claims";

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!; // ID Interno do Banco [cite: 19]

        // Este ÃƒÂ© o ID real que o MP usa (ex: 5012391221)
        [MongoIndex(Unique = true)]
        public long MpClaimId { get; set; } // Mudei para long pois geralmente ÃƒÂ© numÃƒÂ©rico, mas string tambÃƒÂ©m funciona [cite: 20]

        // ID do pagamento vinculado (Resource ID)
        // CORREÃƒâ€¡ÃƒÆ’O: De 'ResorceId' para 'ResourceId'
        public string? ResourceId { get; set; }

        // Agora usando o Enum forte em vez de string
        public ClaimType Type { get; set; } // "mediations", "cancel_purchase" etc [cite: 22]

        // Agora usando o Enum forte em vez de string
        public ClaimResource? ResourceType { get; set; } // "payment", "subscription" etc [cite: 23]

        // Novo campo sugerido para guardar o status original do MP (opened/closed) separadamente do seu status interno
        public ClaimStage? CurrentStage { get; set; } // claim, dispute, etc [cite: 4]

        public DateTime DataCreated { get; set; } = DateTime.UtcNow;

        // Seu status interno de controle
        public InternalClaimStatus Status { get; set; } = InternalClaimStatus.Novo; 

        public string? MercadoPagoPanelUrl =>
            $"https://www.mercadopago.com.br/developers/panel/notifications/claims/{MpClaimId}"; 

        public string? UserId { get; set; }
    
        [BsonIgnore]
        public virtual Users? User { get; set; }
    }
}

