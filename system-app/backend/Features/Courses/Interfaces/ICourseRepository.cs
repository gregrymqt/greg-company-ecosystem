using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.Courses.Interfaces
{
    public interface ICourseRepository
    {
        Task<Course?> GetByPublicIdAsync(Guid publicId);
        Task<Course?> GetByPublicIdWithVideosAsync(Guid publicId);
        Task<Course?> GetByNameAsync(string name);
        Task<IEnumerable<Course>> SearchByNameAsync(string name);
        Task<bool> ExistsByNameAsync(string name);

        Task<(IEnumerable<Course> Items, int TotalCount)> GetPaginatedWithVideosAsync(
            int pageNumber,
            int pageSize
        );

        Task AddAsync(Course course);
        void Update(Course course);
        void Delete(Course course);
    }
}
