using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.Profiles.Admin.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.Profiles.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/students")]
    public class AdminStudentsController : ApiControllerBase
    {
        private readonly IAdminStudentService _studentService;

        public AdminStudentsController(IAdminStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudents(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var paginatedStudents = await _studentService.GetAllStudentsAsync(page, pageSize);
                return Ok(paginatedStudents);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao buscar alunos.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(Guid id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                return Ok(student);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao buscar aluno.");
            }
        }
    }
}
