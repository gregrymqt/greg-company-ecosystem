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
    // 1. A estrutura de Mocks continua igual (necessária para o Setup)
    private readonly Mock<IAboutRepository> _aboutRepositoryMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly AboutService _aboutService;

    public GetAboutAsyncTests()
    {
        _aboutService = new AboutService(
            _aboutRepositoryMock.Object,
            _cacheServiceMock.Object,
            new Mock<IFileService>().Object,
            new Mock<IUnitOfWork>().Object
        );
    }

    public AboutPageContentDto CreateFakeDto() => new()
    {
        Sections =
        [
            new AboutSectionDto
            {
                Id = 1,
                Title = "Cached Section",
                Description = "Cached Description",
                ImageUrl = "cached_url",
                ImageAlt = "cached_alt",
            }

        ],
        TeamSection = new AboutTeamSectionDto
        {
            Members =
            [
                new TeamMemberDto
                {
                    Id = 1,
                    Name = "Cached John Doe",
                    Role = "Cached Developer",
                    PhotoUrl = "cached_photo",
                    LinkedinUrl = "https://linkedin.com/in/cachedjohndoe",
                    GithubUrl = "https://github.com/cachedjohndoe",
                }

            ],
        },
    };

    [Theory]
    [InlineData(true)] // Cenário 1: Cache tem dados (Hit)
    [InlineData(false)] // Cenário 2: Cache está vazio (Miss)
    public async Task GetAboutPageContent_ShouldHandleCacheFlow(bool isCacheHit)
    {
        // ARRANGE
        var cachedDto = isCacheHit
            ? CreateFakeDto()
            : null;

        _cacheServiceMock
            .Setup(c => c.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<AboutPageContentDto>>>(),
                It.IsAny<TimeSpan?>()))
            .Returns(async (string key, Func<Task<AboutPageContentDto>> factory, TimeSpan? expiry) =>
                isCacheHit ? cachedDto : await factory());

        // Se for Miss (false), precisamos que o Repo retorne algo para a factory não quebrar
        _aboutRepositoryMock.Setup(r => r.GetAllSectionsAsync()).ReturnsAsync([]);
        _aboutRepositoryMock.Setup(r => r.GetAllTeamMembersAsync()).ReturnsAsync([]);

        // ACT
        var result = await _aboutService.GetAboutPageContentAsync();

        // ASSERT
        Assert.NotNull(result);

        // A mágica: se isCacheHit é true, o repo deve ser chamado ZERO vezes (Never). 
        // Se for false, deve ser chamado UMA vez (Once).
        var expectedCalls = isCacheHit ? Times.Never() : Times.Once();
        _aboutRepositoryMock.Verify(r => r.GetAllSectionsAsync(), expectedCalls);
    }
}