using MeuCrudCsharp.Features.Auth.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities
{
    public class Subscription : TransactionBase
    {
        public Guid PlanId { get; set; }

        [NotMapped]
        public string? MercadoPagoPreapprovalId
        {
            get => ExternalId;
            set => ExternalId = value;
        }

        [NotMapped]
        public SubscriptionStatus SubscriptionStatus
        {
            get => Status != null ? SubscriptionStatusExtensions.FromMpString(Status) : SubscriptionStatus.Unknown;
            set => Status = value.ToMpString();
        }

        [ForeignKey(nameof(PlanId))]
        public virtual Plan? Plan { get; set; }

        [NotMapped]
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
