using System.Collections.Generic;
using System.Linq;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.DTOs;
using MeuCrudCsharp.Features.Profiles.UserAccount.DTOs;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.ViewModels
{
    public class ProfileViewModel
    {
        public UserProfileDto? UserProfile { get; set; }

        public SubscriptionDetailsDto? Subscription { get; set; }

        public IEnumerable<Models.Payments> PaymentHistory { get; set; } =
            Enumerable.Empty<Models.Payments>();
    }
}
