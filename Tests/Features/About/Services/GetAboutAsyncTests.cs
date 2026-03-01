using System;
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

namespace Tests.Features.About.Services;

public class GetAboutAsyncTests : AboutServiceTestBase
{
    public AboutPageContentDto CreateFakeAboutPageContentDto() =>
        new()
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
                },
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
                    },
                ],
            },
        };

    [Theory]
    [InlineData(true)] // Scenario 1: Cache has data (Hit)
    [InlineData(false)] // Scenario 2: Cache is empty (Miss)
    public async Task GetAboutPageContent_ShouldHandleCacheFlow(bool isCacheHit)
    {
        // ARRANGE
        var cachedDto = isCacheHit ? CreateFakeAboutPageContentDto() : null;

        _cache
            .Setup(c =>
                c.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<AboutPageContentDto>>>(),
                    It.IsAny<TimeSpan?>()
                )
            )
            .Returns(
                async (string key, Func<Task<AboutPageContentDto>> factory, TimeSpan? expiry) =>
                    isCacheHit ? cachedDto : await factory()
            );

        _repository.Setup(r => r.GetAllSectionsAsync()).ReturnsAsync([]);
        _repository.Setup(r => r.GetAllTeamMembersAsync()).ReturnsAsync([]);

        // ACT
        var result = await _sut.GetAboutPageContentAsync();

        // ASSERT
        Assert.NotNull(result);

        // If isCacheHit is true, the repo should be called ZERO times (Never).
        // If false, it should be called exactly ONE time (Once).
        var expectedCalls = isCacheHit ? Times.Never() : Times.Once();
        _repository.Verify(r => r.GetAllSectionsAsync(), expectedCalls);
    }

    [Fact]
    public async Task GetAboutPageContent_WhenRepositoryFails_ShouldPropagateException()
    {
        // Arrange
        _cache
            .Setup(c =>
                c.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<AboutPageContentDto>>>(),
                    It.IsAny<TimeSpan?>()
                )
            )
            .Returns(
                async (string key, Func<Task<AboutPageContentDto>> factory, TimeSpan? expiry) =>
                    await factory()
            );

        _repository
            .Setup(r => r.GetAllSectionsAsync())
            .ThrowsAsync(new Exception("Falha de conexão com o banco de dados"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAboutPageContentAsync());

        Assert.Equal("Falha de conexão com o banco de dados", exception.Message);
    }
}
