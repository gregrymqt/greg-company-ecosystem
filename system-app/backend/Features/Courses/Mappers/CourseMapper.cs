using MeuCrudCsharp.Features.Courses.DTOs;
using MeuCrudCsharp.Features.Videos.DTOs;
using MeuCrudCsharp.Features.Videos.Utils;

namespace MeuCrudCsharp.Features.Courses.Mappers
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
