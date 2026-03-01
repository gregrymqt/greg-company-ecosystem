using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.Profiles.Admin.Interfaces
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
