using System.Collections.Generic;
using System.Linq;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Application.DTOs;
using MeuCrudCsharp.Features.Profiles.UserAccount.Application.DTOs;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.Application.ViewModels
{
    public class ProfileViewModel
    {
        public UserProfileDto? UserProfile { get; set; }

        public SubscriptionDetailsDto? Subscription { get; set; }

        public IEnumerable<MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities.Payments> PaymentHistory { get; set; } =
            Enumerable.Empty<MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities.Payments>();
    }
}



