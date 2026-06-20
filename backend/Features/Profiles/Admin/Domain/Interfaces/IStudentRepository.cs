using MeuCrudCsharp.Features.Profiles.Admin.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Auth.Domain.Entities;

namespace MeuCrudCsharp.Features.Profiles.Admin.Domain.Interfaces
{
    public interface IStudentRepository
    {
        Task<(IEnumerable<Users> Items, int TotalCount)> GetAllWithSubscriptionsAsync(
            int page,
            int pageSize
        );

        Task<Users?> GetByPublicIdWithSubscriptionAsync(Guid publicId);
    }
}

