using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.Profiles.Admin.Application.Dtos;

namespace MeuCrudCsharp.Features.Profiles.Admin.Application.Interfaces
{
    public interface IAdminStudentService
    {
        Task<PaginatedResult<StudentDto>> GetAllStudentsAsync(int page, int pageSize);


        Task<StudentDto> GetStudentByIdAsync(Guid id);
    }
}
