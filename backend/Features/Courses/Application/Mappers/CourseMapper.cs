using MeuCrudCsharp.Features.Courses.Application.DTOs;
using MeuCrudCsharp.Features.Videos.Application.DTOs;
using MeuCrudCsharp.Features.Videos.Application.Utils;

namespace MeuCrudCsharp.Features.Courses.Application.Mappers
{
    public static class CourseMapper
    {
        public static CourseDto ToDtoWithVideos(Models.Course course)
        {
            return new CourseDto
            {
                PublicId = course.PublicId,
                Name = course.Name,
                Description = course.Description,
                Videos = course.Videos?.Select(VideoMapper.ToDto).ToList() ?? new List<VideoDto>(),
            };
        }

        public static CourseDto ToDto(Models.Course course)
        {
            return new CourseDto
            {
                PublicId = course.PublicId,
                Name = course.Name,
                Description = course.Description,
            };
        }
    }
}
