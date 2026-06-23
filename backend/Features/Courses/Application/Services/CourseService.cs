using MeuCrudCsharp.Features.Caching.Application.Interfaces;
using MeuCrudCsharp.Features.Courses.Application.DTOs;
using MeuCrudCsharp.Features.Courses.Application.Interfaces;
using MeuCrudCsharp.Features.Courses.Domain.Interfaces;
using MeuCrudCsharp.Features.Courses.Application.Mappers;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Features.Videos.Application.DTOs;
using MeuCrudCsharp.Features.Shared.Domain.Entities; 
using MeuCrudCsharp.Features.Courses.Domain.Entities;
using MeuCrudCsharp.Data;
using System.Text.Json;

namespace MeuCrudCsharp.Features.Courses.Application.Services
{
    public class CourseService(
        ICourseRepository repository,
        ILogger<CourseService> logger,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        IMongoDbContext mongoContext
    ) : ICourseService
    {
        private const string CoursesCacheVersionKey = "courses_cache_version";

        public async Task<IEnumerable<CourseDto>> SearchCoursesByNameAsync(string name)
        {
            var courses = await repository.SearchByNameAsync(name);
            return courses.Select(CourseMapper.ToDto);
        }

        public async Task<PaginatedResultDto<CourseDto>> GetCoursesWithVideosPaginatedAsync(
            int pageNumber,
            int pageSize
        )
        {
            var cacheVersion = await GetCacheVersionAsync();
            var cacheKey = $"Courses_v{cacheVersion}_Page{pageNumber}_Size{pageSize}";

            return await cacheService.GetOrCreateAsync(
                    cacheKey,
                    async () =>
                    {
                        logger.LogInformation("Buscando cursos do banco (cache miss)...");

                        var (items, totalCount) = await repository.GetPaginatedWithVideosAsync(
                            pageNumber,
                            pageSize
                        );

                        var dtos = items.Select(CourseMapper.ToDtoWithVideos).ToList();

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
                throw new AppServiceException("Já existe um curso com este nome.");
            }

            var newCourse = new Course
            {
                Id = Guid.NewGuid().ToString(),
                Name = createDto.Name!,
                Description = createDto.Description ?? string.Empty,
            };

            using var session = await mongoContext.StartSessionAsync();
            session.StartTransaction();
            try
            {
                // 1. Grava o Dado Principal
                var coursesCollection = mongoContext.GetCollection<Course>("Courses");
                await coursesCollection.InsertOneAsync(session, newCourse);

                // 2. Grava no Outbox
                var outboxEvent = new OutboxEvent
                {
                    EventType = "CourseCreated",
                    Payload = JsonSerializer.Serialize(new { CourseId = newCourse.Id, newCourse.Name })
                };
                var outboxCollection = mongoContext.GetCollection<OutboxEvent>("OutboxEvents");
                await outboxCollection.InsertOneAsync(session, outboxEvent);

                // 3. Commit de forma atômica
                await session.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                logger.LogError(ex, "Erro ao criar curso e salvar no Outbox.");
                throw;
            }

            // Invalida cache e loga
            await cacheService.InvalidateCacheByKeyAsync(CoursesCacheVersionKey);
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

            repository.Update(course);
            await unitOfWork.CommitAsync();

            await cacheService.InvalidateCacheByKeyAsync(CoursesCacheVersionKey);

            logger.LogInformation("Curso {CourseId} atualizado.", publicId);
            return CourseMapper.ToDto(course);
        }

        public async Task DeleteCourseAsync(Guid publicId)
        {
            var course = await repository.GetByPublicIdWithVideosAsync(publicId)
                ?? throw new ResourceNotFoundException($"Curso com ID {publicId} não encontrado.");

            if (course.Videos.Count != 0)
            {
                throw new AppServiceException(
                    "Não é possível deletar um curso que possui vídeos associados."
                );
            }

            repository.Delete(course);
            await unitOfWork.CommitAsync();

            await cacheService.InvalidateCacheByKeyAsync(CoursesCacheVersionKey);

            logger.LogInformation("Curso {CourseId} deletado.", publicId);
        }

        public async Task<Course> FindCourseByPublicIdOrFailAsync(Guid publicId)
        {
            var course = await repository.GetByPublicIdAsync(publicId)
                ?? throw new ResourceNotFoundException(
                    $"Curso com o PublicId {publicId} não encontrado."
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
