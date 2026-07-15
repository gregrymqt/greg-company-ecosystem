using MeuCrudCsharp.Features.Courses.Application.DTOs;
using MeuCrudCsharp.Features.Courses.Domain.Entities;
using MeuCrudCsharp.Features.Videos.Application.DTOs;
using MeuCrudCsharp.Features.Videos.Application.Utils;

namespace MeuCrudCsharp.Features.Courses.Application.Mappers
{
    public static class CourseMapper
    {
        public static CourseDto ToDtoWithModules(Course course)
        {
            return new CourseDto
            {
                PublicId = course.PublicId,
                Name = course.Name,
                Description = course.Description,
                Year = course.Year,
                Creator = course.Creator,
                Price = course.Price,
                IsPublished = course.IsPublished,
                ThumbnailUrl = course.ThumbnailUrl,
                Modules = course.Modules?.Select(m => new ModuleDto
                {
                    PublicId = m.PublicId,
                    Title = m.Title,
                    Order = m.Order,
                    Lessons = m.Lessons?.Select(l => new LessonDto
                    {
                        PublicId = l.PublicId,
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
                PublicId = course.PublicId,
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
