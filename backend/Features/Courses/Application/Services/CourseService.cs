using MeuCrudCsharp.Features.Caching.Application.Interfaces;
using MeuCrudCsharp.Features.Courses.Application.DTOs;
using MeuCrudCsharp.Features.Courses.Application.Interfaces;
using MeuCrudCsharp.Features.Courses.Domain.Interfaces;
using MeuCrudCsharp.Features.Courses.Application.Mappers;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using MeuCrudCsharp.Features.Courses.Domain.Entities;
using MeuCrudCsharp.Features.Videos.Application.DTOs;
using MeuCrudCsharp.Data;
using System.Text.Json;

namespace MeuCrudCsharp.Features.Courses.Application.Services
{
    public class CourseService(
        ICourseRepository repository,
        ILogger<CourseService> logger,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        ApplicationDbContext dbContext,
        Microsoft.Extensions.Configuration.IConfiguration configuration
    ) : ICourseService
    {
        private const string CoursesCacheVersionKey = "courses_cache_version";

        public async Task<IEnumerable<CourseDto>> SearchCoursesByNameAsync(string name)
        {
            var courses = await repository.SearchByNameAsync(name);
            return courses.Select(CourseMapper.ToDto);
        }

        public async Task<PaginatedResultDto<CourseDto>> GetCoursesWithModulesPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            bool onlyPublished = false
        )
        {
            var cacheVersion = await GetCacheVersionAsync();
            var cacheKey = $"Courses_v{cacheVersion}_Page{pageNumber}_Size{pageSize}_Name{name ?? "none"}_Pub{onlyPublished}";

            return await cacheService.GetOrCreateAsync(
                    cacheKey,
                    async () =>
                    {
                        logger.LogInformation("Buscando cursos do banco (cache miss)...");

                        var (items, totalCount) = await repository.GetPaginatedWithModulesAsync(
                            pageNumber,
                            pageSize,
                            name,
                            onlyPublished
                        );

                        var dtos = items.Select(CourseMapper.ToDtoWithModules).ToList();

                        return new PaginatedResultDto<CourseDto>(
                            dtos,
                            totalCount,
                            pageNumber,
                            pageSize
                        );
                    },
                    TimeSpan.FromMinutes(10)
                ) ?? throw new AppServiceException("Erro ao obter cursos paginados.");
        }

        public async Task<CourseDto> CreateCourseAsync(CreateUpdateCourseDto createDto)
        {
            if (await repository.ExistsByNameAsync(createDto.Name!))
            {
                throw new AppServiceException("Ja existe um curso com este nome.");
            }

            var newCourse = new Course
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name!,
                Description = createDto.Description ?? string.Empty,
                Price = createDto.Price,
                IsPublished = createDto.IsPublished,
                ThumbnailUrl = createDto.ThumbnailUrl,
                Modules = createDto.Modules?.Select(m => new Module
                {
                    Title = m.Title,
                    Order = m.Order,
                    Lessons = m.Lessons?.Select(l => new Lesson
                    {
                        Title = l.Title,
                        Order = l.Order,
                        VideoPublicId = l.VideoPublicId,
                        VideoTitle = l.VideoTitle
                    }).ToList() ?? new List<Lesson>()
                }).ToList() ?? new List<Module>()
            };

            await unitOfWork.BeginTransactionAsync();
            try
            {
                await dbContext.Courses.AddAsync(newCourse);

                var outboxEvent = new OutboxEvent
                {
                    EventType = "CourseCreated",
                    Payload = JsonSerializer.Serialize(new { CourseId = newCourse.Id, newCourse.Name })
                };
                await dbContext.OutboxEvents.AddAsync(outboxEvent);

                await unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                logger.LogError(ex, "Erro ao criar curso e salvar no Outbox.");
                throw;
            }

            await cacheService.InvalidateCacheByKeyAsync(CoursesCacheVersionKey);
            if (configuration["USE_REDIS"] == "true")
            {
                await cacheService.InvalidateCacheByKeyAsync("catalog:courses:public");
            }
            logger.LogInformation("Novo curso '{CourseName}' criado e evento enviado ao Outbox.", newCourse.Name);

            return CourseMapper.ToDto(newCourse);
        }

        public async Task<CourseDto> UpdateCourseAsync(
            Guid publicId,
            CreateUpdateCourseDto updateDto
        )
        {
            var course = await FindCourseByPublicIdOrFailAsync(publicId);

            course.Name = updateDto.Name!;
            course.Description = updateDto.Description ?? string.Empty;
            course.Price = updateDto.Price;
            course.IsPublished = updateDto.IsPublished;
            course.ThumbnailUrl = updateDto.ThumbnailUrl;
            course.Modules = updateDto.Modules?.Select(m => new Module
            {
                Title = m.Title,
                Order = m.Order,
                Lessons = m.Lessons?.Select(l => new Lesson
                {
                    Title = l.Title,
                    Order = l.Order,
                    VideoPublicId = l.VideoPublicId,
                    VideoTitle = l.VideoTitle
                }).ToList() ?? new List<Lesson>()
            }).ToList() ?? new List<Module>();

            repository.Update(course);
            await unitOfWork.CommitAsync();

            await cacheService.InvalidateCacheByKeyAsync(CoursesCacheVersionKey);
            if (configuration["USE_REDIS"] == "true")
            {
                await cacheService.InvalidateCacheByKeyAsync("catalog:courses:public");
            }

            logger.LogInformation("Curso {CourseId} atualizado.", publicId);
            return CourseMapper.ToDto(course);
        }

        public async Task DeleteCourseAsync(Guid publicId)
        {
            var course = await repository.GetByPublicIdWithModulesAsync(publicId)
                ?? throw new ResourceNotFoundException($"Curso com ID {publicId} nao encontrado.");

            if (course.Modules.Count != 0)
            {
                throw new AppServiceException(
                    "Nao e possivel deletar um curso que possui modulos associados."
                );
            }

            repository.Delete(course);
            await unitOfWork.CommitAsync();

            await cacheService.InvalidateCacheByKeyAsync(CoursesCacheVersionKey);
            if (configuration["USE_REDIS"] == "true")
            {
                await cacheService.InvalidateCacheByKeyAsync("catalog:courses:public");
            }

            logger.LogInformation("Curso {CourseId} deletado.", publicId);
        }

        public async Task<Course> FindCourseByPublicIdOrFailAsync(Guid publicId)
        {
            var course = await repository.GetByPublicIdAsync(publicId)
                ?? throw new ResourceNotFoundException(
                    $"Curso com o PublicId {publicId} nao encontrado."
                );

            return course;
        }

        public async Task<Course> GetOrCreateCourseByNameAsync(string courseName)
        {
            if (string.IsNullOrWhiteSpace(courseName))
                throw new ArgumentException("Nome vazio.", nameof(courseName));

            var course = await repository.GetByNameAsync(courseName);

            if (course != null)
                return course;

            logger.LogInformation("Criando curso '{CourseName}'...", courseName);
            course = new Course { Name = courseName };

            await repository.AddAsync(course);

            return course;
        }

        private Task<string?> GetCacheVersionAsync()
        {
            return cacheService.GetOrCreateAsync(
                CoursesCacheVersionKey,
                () => Task.FromResult(Guid.NewGuid().ToString()),
                TimeSpan.FromDays(30)
            );
        }
    }
}
