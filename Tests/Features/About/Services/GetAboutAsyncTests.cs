using MeuCrudCsharp.Features.About.Application.DTOs;
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

    [Fact]
    public async Task GetAboutPageContent_WhenCacheHit_ShouldReturnCachedDataWithoutCallingRepository()
    {
        // Arrange
        var cachedDto = CreateFakeAboutPageContentDto();

        _cache
            .Setup(c =>
                c.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<AboutPageContentDto>>>(),
                    It.IsAny<TimeSpan?>()
                )
            )
            .ReturnsAsync(cachedDto);

        // Act
        var result = await _sut.GetAboutPageContentAsync();

        // Assert
        Assert.NotNull(result);
        _repository.Verify(r => r.GetAllSectionsAsync(), Times.Never);
        _repository.Verify(r => r.GetAllTeamMembersAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAboutPageContent_WhenCacheMiss_ShouldCallRepositoryAndReturnData()
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

        _repository.Setup(r => r.GetAllSectionsAsync()).ReturnsAsync([]);
        _repository.Setup(r => r.GetAllTeamMembersAsync()).ReturnsAsync([]);

        // Act
        var result = await _sut.GetAboutPageContentAsync();

        // Assert
        Assert.NotNull(result);
        _repository.Verify(r => r.GetAllSectionsAsync(), Times.Once);
        _repository.Verify(r => r.GetAllTeamMembersAsync(), Times.Once);
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
