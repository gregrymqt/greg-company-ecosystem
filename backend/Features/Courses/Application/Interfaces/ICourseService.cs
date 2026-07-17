using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.Courses.Application.DTOs;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Shared.Application.DTOs;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Courses.Domain.Entities;

namespace MeuCrudCsharp.Features.Courses.Application.Interfaces
{
    public interface ICourseService
    {
        Task<PaginatedResultDto<CourseDto>> GetCoursesWithModulesPaginatedAsync(
            int pageNumber,
            int pageSize, string? name = null,
            bool onlyPublished = false
        );

        Task<IEnumerable<CourseDto>> SearchCoursesByNameAsync(string name);

        Task<CourseDto> CreateCourseAsync(CreateUpdateCourseDto createDto);

        Task<CourseDto> UpdateCourseAsync(Guid id, CreateUpdateCourseDto updateDto);

        Task DeleteCourseAsync(Guid id);

        Task<Course> FindCourseByPublicIdOrFailAsync(Guid publicId);

        Task<Course> GetOrCreateCourseByNameAsync(string courseName);
    }
}

