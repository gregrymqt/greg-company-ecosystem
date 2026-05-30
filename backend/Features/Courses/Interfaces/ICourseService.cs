using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.Courses.DTOs;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Videos.DTOs;
using MeuCrudCsharp.Models;

namespace MeuCrudCsharp.Features.Courses.Interfaces
{
    public interface ICourseService
    {
        Task<PaginatedResultDto<CourseDto>> GetCoursesWithVideosPaginatedAsync(
            int pageNumber,
            int pageSize
        );

        Task<IEnumerable<CourseDto>> SearchCoursesByNameAsync(string name);

        Task<CourseDto> CreateCourseAsync(CreateUpdateCourseDto createDto);

        Task<CourseDto> UpdateCourseAsync(Guid id, CreateUpdateCourseDto updateDto);

        Task DeleteCourseAsync(Guid id);

        Task<Course> FindCourseByPublicIdOrFailAsync(Guid publicId);

        Task<Course> GetOrCreateCourseByNameAsync(string courseName);
    }
}
