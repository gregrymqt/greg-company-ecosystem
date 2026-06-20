using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace MeuCrudCsharp.Models
{
                         // <-- NOVO: Indexar a data de expiração é ótimo para performance
    public class Subscription : TransactionBase, IMongoDocument
    {
        public static string CollectionName => "subscriptions";

        // --- RELACIONAMENTO COM PLAN ---
        public string PlanId { get; set; }

        [BsonIgnore]
        public virtual Plan? Plan { get; set; }

        [BsonIgnore]
        public Guid PlanPublicId { get; set; }

        public string? LastFourCardDigits { get; set; }

        public string? PayerMpId { get; set; }

        public int CurrentAmount { get; set; }

        public DateTime CurrentPeriodStartDate { get; set; }

        public DateTime CurrentPeriodEndDate { get; set; }

        public string? PaymentMethodId { get; set; }

        public string? CardTokenId { get; set; }
    }
}


