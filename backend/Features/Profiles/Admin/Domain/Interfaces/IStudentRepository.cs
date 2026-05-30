using MeuCrudCsharp.Features.Profiles.Admin.Domain.Interfaces;
using MeuCrudCsharp.Models;
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
