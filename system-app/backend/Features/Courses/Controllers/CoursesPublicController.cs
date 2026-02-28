using System;
using System.Threading.Tasks;
using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.Courses.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeuCrudCsharp.Features.Courses.Controllers
{
    [Route("api/public/courses")]
    public class PublicCoursesController : ApiControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ILogger<PublicCoursesController> _logger;

        public PublicCoursesController(
            ICourseService courseService,
            ILogger<PublicCoursesController> logger
        )
        {
            _courseService = courseService;
            _logger = logger;
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetCoursesPaginated(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5
        )
        {
            try
            {
                var paginatedResult = await _courseService.GetCoursesWithVideosPaginatedAsync(
                    pageNumber,
                    pageSize
                );
                return Ok(paginatedResult);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Não foi possível carregar os cursos no momento.");
            }
        }
    }
}
