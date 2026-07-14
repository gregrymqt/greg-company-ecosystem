using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
// Models/Plan.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MeuCrudCsharp.Data.Configuration.Interfaces;
using MeuCrudCsharp.Data.Configuration.Attributes;


// Enums/PlanFrequencyType.cs
// Este enum representa os valores que a API do Mercado Pago aceita para a frequência.

public enum PlanFrequencyType
{
    Days, // Corresponde a "days" na API
    Months, // Corresponde a "months" na API
}

namespace MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities
{
    public class Plan : IMongoDocument
    {
        public static string CollectionName => "plans";

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public Guid PublicId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// ID do plano correspondente no Mercado Pago.
        /// Essencial para criar assinaturas vinculadas a ele.
        /// </summary>
        [MongoIndex(Unique = true)]
        public string ExternalPlanId { get; set; } = string.Empty;

        public required string Name { get; set; }

        public string? Description { get; set; }
        
        public string Category { get; set; } = "course";

        /// <summary>
        /// Valor da transação/cobrança. Mapeia para "transaction_amount" na API.
        /// </summary>
        public decimal TransactionAmount { get; set; }

        /// <summary>
        /// Moeda da transação. Mapeia para "currency_id" na API.
        /// </summary>
        public string CurrencyId { get; set; } = "BRL";

        // --- CAMPOS DE FREQUÊNCIA REFINADOS ---

        /// <summary>
        /// O intervalo da recorrência. Ex: 1, 3, 6.
        /// Mapeia para o campo "frequency" na API do Mercado Pago.
        /// </summary>
        public int FrequencyInterval { get; set; }

        /// <summary>
        /// A unidade de tempo da recorrência (Dias ou Meses).
        /// Mapeia para o campo "frequency_type" na API do Mercado Pago.
        /// </summary>
        public PlanFrequencyType FrequencyType { get; set; }

        public bool IsActive { get; set; } = false;
    }
}
