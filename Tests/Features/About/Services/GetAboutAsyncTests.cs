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
    [InlineData(true)] // Cenário 1: Cache tem dados (Hit)
    [InlineData(false)] // Cenário 2: Cache está vazio (Miss)
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

        // Se for Miss (false), precisamos que o Repo retorne algo para a factory não quebrar
        _repository.Setup(r => r.GetAllSectionsAsync()).ReturnsAsync([]);
        _repository.Setup(r => r.GetAllTeamMembersAsync()).ReturnsAsync([]);

        // ACT
        var result = await _sut.GetAboutPageContentAsync();

        // ASSERT
        Assert.NotNull(result);

        // A mágica: se isCacheHit é true, o repo deve ser chamado ZERO vezes (Never).
        // Se for false, deve ser chamado UMA vez (Once).
        var expectedCalls = isCacheHit ? Times.Never() : Times.Once();
        _repository.Verify(r => r.GetAllSectionsAsync(), expectedCalls);
    }

    [Fact]
    public async Task GetAboutPageContent_QuandoRepositorioFalhar_DeveSubirExcecao()
    {
        // 1. Plantamos a bomba: O Repositório vai dar erro!
        _repository
            .Setup(r => r.GetAllSectionsAsync())
            .ThrowsAsync(new Exception("Falha de conexão com o banco de dados"));

        // 2 e 3. Armamos a rede do xUnit e chamamos o método de verdade lá dentro
        var excecao = await Assert.ThrowsAsync<Exception>(() => _sut.GetAboutPageContentAsync());

        // Opcional: Validamos se a bomba que explodiu foi a mesma que plantamos
        Assert.Equal("Falha de conexão com o banco de dados", excecao.Message);
    }
}
