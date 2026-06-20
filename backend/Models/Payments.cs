using System;
using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace MeuCrudCsharp.Models
{
         // A FK já é um Guid (string), então está ok.
    public class Payments : TransactionBase, IMongoDocument
    {
        public static string CollectionName => "payments";

        public string? Method { get; set; }

        public int Installments { get; set; }

        public DateTime? DateApproved { get; set; }

        public string? LastFourDigits { get; set; }

        [MongoIndex]
        public string? CustomerCpf { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; } = null!;

        public string SubscriptionId { get; set; } = null!;

        [BsonIgnore]
        public virtual Subscription? Subscription { get; set; }
    }
}

