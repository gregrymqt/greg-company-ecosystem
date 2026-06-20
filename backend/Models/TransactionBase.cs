using System;
using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MeuCrudCsharp.Models
{
    public abstract class TransactionBase
    {
        // A chave primária já é um Guid em formato de string, o que é seguro para expor.
        // Neste caso, ele serve tanto como PK quanto como identificador público.
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        // ID externo (do Mercado Pago, por exemplo)
        public string? ExternalId { get; set; }

        // A FK para o usuário já é uma string (padrão do Identity), então está correto.
        public required string UserId { get; set; }

        [BsonIgnore]
        public virtual Users? User { get; set; }

        public string? Status { get; set; }

        public string? PayerEmail { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? PaymentId { get; set; }

        protected TransactionBase()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
