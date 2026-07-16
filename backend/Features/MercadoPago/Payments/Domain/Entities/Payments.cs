using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities
{
    public class Payment : TransactionBase
    {
        public string? Method { get; set; }

        public int Installments { get; set; }

        public DateTime? DateApproved { get; set; }

        public string? LastFourDigits { get; set; }

        public string? CustomerCpf { get; set; }

        public decimal Amount { get; set; }

        public decimal? NetReceivedAmount { get; set; }

        public string Description { get; set; } = null!;

        public Guid SubscriptionId { get; set; }

        [ForeignKey(nameof(SubscriptionId))]
        public virtual Subscription? Subscription { get; set; }
    }
}
