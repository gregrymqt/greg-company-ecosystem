using Humanizer;
using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Features.About.Interfaces;
using MeuCrudCsharp.Features.About.Services;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Files.Services;
using MeuCrudCsharp.Features.Shared.Work;
using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using System;

namespace Tests.Features.About.Services;

public class GetAboutAsyncTests
{
    private readonly Mock<IAboutRepository> _aboutRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IWebHostEnvironment> _WebHostEnvironmentMock;

    private readonly AboutService _aboutService;
    private readonly FileService _fileService;

    private const string ABOUT_CACHE_KEY = "ABOUT_PAGE_CONTENT";
    private const string CAT_SECTION = "AboutSection"; // Categorias para organizar arquivos
    private const string CAT_TEAM = "AboutTeam";

    public GetAboutAsyncTests()
    {
        _aboutRepositoryMock = new Mock<IAboutRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _fileServiceMock = new Mock<IFileService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fileRepositoryMock = new Mock<IFileRepository>();
        _WebHostEnvironmentMock = new Mock<IWebHostEnvironment>();

        _aboutService = new AboutService(
            _aboutRepositoryMock.Object,
            _cacheServiceMock.Object,
            _fileServiceMock.Object,
            _unitOfWorkMock.Object
        );

       _fileService = new FileService(
           _fileRepositoryMock.Object,
           _WebHostEnvironmentMock.Object,
           _unitOfWorkMock.Object);
    }

    // =================================================================
    // TESTES Cache
    // =================================================================
    [Fact]
    public async Task GetAboutPageContentAsync_WhenCacheIsEmpty_ShouldFetchFromRepositoryAndReturnDto()
    {
        // =================================================================
        // ARRANGE (Organizar)
        // =================================================================

        // 1. Crie os dados que os repositórios devem retornar (Models, não DTOs).
        var sectionsFromRepo = new List<AboutSection>
        {
            new()
            {
                Id = 1,
                Title = "Section 1",
                Description = "Description 1",
                ImageUrl = "url1",
                ImageAlt = "alt1",
            },
        };
        var membersFromRepo = new List<TeamMember>
        {
            new()
            {
                Id = 1,
                Name = "John Doe",
                Role = "Developer",
                PhotoUrl = "photo1",
                LinkedinUrl = "https://linkedin.com/in/johndoe",
                GithubUrl = "https://github.com/johndoe",
            },
        };

        // 2. Configure os mocks dos repositórios para retornarem esses dados.
        _aboutRepositoryMock
            .Setup(repo => repo.GetAllSectionsAsync())
            .ReturnsAsync(sectionsFromRepo);
        _aboutRepositoryMock
            .Setup(repo => repo.GetAllTeamMembersAsync())
            .ReturnsAsync(membersFromRepo);

        // 3. Configure o mock do cache para simular um "cache miss".
        //    Isso força a execução da função "factory" que busca os dados do repositório.
        _cacheServiceMock
            .Setup(cache =>
                cache.GetOrCreateAsync(
                    ABOUT_CACHE_KEY, // key
                    It.IsAny<Func<Task<AboutPageContentDto>>>(), // factory
                    It.IsAny<TimeSpan?>() // optional expiration
                )
            )
            // Executa a factory para simular um cache miss. A assinatura do Returns agora corresponde ao Setup.
            .Returns(
                (string key, Func<Task<AboutPageContentDto>> factory, TimeSpan? expiry) => factory()
            );

        // =================================================================
        // ACT (Agir)
        // =================================================================
        var result = await _aboutService.GetAboutPageContentAsync();

        // =================================================================
        // ASSERT (Verificar)
        // =================================================================
        Assert.NotNull(result);
        Assert.Single(result.Sections);
        Assert.Equal("Section 1", result.Sections[0].Title);
        Assert.Single(result.TeamSection.Members);
        Assert.Equal("John Doe", result.TeamSection.Members[0].Name);

        // Verifique se os métodos do repositório foram realmente chamados (confirmando o "cache miss").
        _aboutRepositoryMock.Verify(repo => repo.GetAllSectionsAsync(), Times.Once);
        _aboutRepositoryMock.Verify(repo => repo.GetAllTeamMembersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAboutPageContentAsync_WhenCacheIsHit_ShouldReturnCachedDataWithoutCallingRepository()
    {
        // =================================================================
        // ARRANGE (Organizar)
        // =================================================================
        // 1. Crie um DTO de conteúdo da página "About" que o cache deve retornar.
        var cachedContent = new AboutPageContentDto
        {
            Sections = new List<AboutSectionDto>
            {
                new()
                {
                    Id = 1,
                    Title = "Cached Section",
                    Description = "Cached Description",
                    ImageUrl = "cached_url",
                    ImageAlt = "cached_alt",
                },
            },
            TeamSection = new AboutTeamSectionDto
            {
                Members = new List<TeamMemberDto>
                {
                    new()
                    {
                        Id = 1,
                        Name = "Cached John Doe",
                        Role = "Cached Developer",
                        PhotoUrl = "cached_photo",
                        LinkedinUrl = "https://linkedin.com/in/cachedjohndoe",
                        GithubUrl = "https://github.com/cachedjohndoe",
                    },
                },
            },
        };

        // 2. Configure o mock do cache para retornar esse DTO, simulando um "cache hit".
        _cacheServiceMock
            .Setup(cache =>
                cache.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<AboutPageContentDto>>>(),
                    It.IsAny<TimeSpan?>()
                )
            )
            .ReturnsAsync(cachedContent);

        // =================================================================
        // ACT (Agir)

        var result = await _aboutService.GetAboutPageContentAsync();

        // =================================================================
        // ASSERT (Verificar)

        Assert.NotNull(result);
        Assert.Single(result.Sections);
        Assert.Equal("Cached Section", result.Sections[0].Title);

        _cacheServiceMock.Verify(
            cache =>
                cache.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<AboutPageContentDto>>>(),
                    It.IsAny<TimeSpan?>()
                ),
            Times.Once
        );
        _aboutRepositoryMock.Verify(repo => repo.GetAllSectionsAsync(), Times.Never);
        _aboutRepositoryMock.Verify(repo => repo.GetAllTeamMembersAsync(), Times.Never);
    }

    // =================================================================
    // TESTES de Seção e Membro
    // =================================================================

    
}
