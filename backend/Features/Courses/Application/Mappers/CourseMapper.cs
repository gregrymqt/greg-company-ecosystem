using MeuCrudCsharp.Features.Courses.Application.DTOs;
using MeuCrudCsharp.Features.Courses.Domain.Entities;

namespace MeuCrudCsharp.Features.Courses.Application.Mappers
{
    public static class CourseMapper
    {
        public static CourseDto ToDtoWithModules(Course course)
        {
            return new CourseDto
            {
                PublicId = course.Id,
                Name = course.Name,
                Description = course.Description,
                Year = course.Year,
                Creator = course.Creator,
                Price = course.Price,
                IsPublished = course.IsPublished,
                ThumbnailUrl = course.ThumbnailUrl,
                Modules = course.Modules?.Select(m => new ModuleDto
                {
                    PublicId = m.Id,
                    Title = m.Title,
                    Order = m.Order,
                    Lessons = m.Lessons?.Select(l => new LessonDto
                    {
                        PublicId = l.Id,
                        Title = l.Title,
                        Order = l.Order,
                        VideoPublicId = l.VideoPublicId,
                        VideoTitle = l.VideoTitle
                    }).ToList() ?? new List<LessonDto>()
                }).ToList() ?? new List<ModuleDto>()
            };
        }

        public static CourseDto ToDto(Course course)
        {
            return new CourseDto
            {
                PublicId = course.Id,
                Name = course.Name,
                Description = course.Description,
                Year = course.Year,
                Creator = course.Creator,
                Price = course.Price,
                IsPublished = course.IsPublished,
                ThumbnailUrl = course.ThumbnailUrl
            };
        }
    }
}
