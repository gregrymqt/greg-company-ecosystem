using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using System;
using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities
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

        public decimal? NetReceivedAmount { get; set; }

        public string Description { get; set; } = null!;

        public string SubscriptionId { get; set; } = null!;

        [BsonIgnore]
        public virtual Subscription? Subscription { get; set; }
    }
}
