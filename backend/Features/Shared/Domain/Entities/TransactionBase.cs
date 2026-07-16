using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MeuCrudCsharp.Features.Auth.Domain.Entities;

namespace MeuCrudCsharp.Features.Shared.Domain.Entities
{
    public abstract class TransactionBase
    {
        [Key]
        public Guid Id { get; set; }

        public string? ExternalId { get; set; }

        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual Users? User { get; set; }

        public string? Status { get; set; }

        public string? PayerEmail { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? PaymentId { get; set; }

        protected TransactionBase()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
