using System.Collections.Generic;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.Profiles.Admin.Dtos;

namespace MeuCrudCsharp.Features.Profiles.Admin.Interfaces
{
    public interface IAdminStudentService
    {
        Task<PaginatedResult<StudentDto>> GetAllStudentsAsync(int page, int pageSize);


        Task<StudentDto> GetStudentByIdAsync(Guid id);
    }
}
