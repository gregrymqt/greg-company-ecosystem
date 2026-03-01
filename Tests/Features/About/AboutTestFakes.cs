using MeuCrudCsharp.Features.About.DTOs;
using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.Http;
using Moq;

namespace MeuCrudCsharp.Tests.Features.About;

public static class AboutTestFakes
{
    public static AboutSection CreateFakeSectionEntity(int? fileId = null) =>
        new()
        {
            Id = 1,
            Title = "Velho",
            FileId = fileId,
        };

    public static CreateUpdateAboutSectionDto CreateFakeAboutSectionDto(bool isChunk = false) =>
        new()
        {
            Title = "Novo",
            IsChunk = isChunk,
            FileName = "foto.jpg",
            File = new Mock<IFormFile>().Object,
        };

    public static TeamMember CreateFakeTeamMemberEntity(int? fileId = null) =>
        new()
        {
            Id = 1,
            Name = "Lucas Vicente",
            Role = "Developer",
            FileId = fileId,
        };

    public static CreateUpdateTeamMemberDto CreateFakeTeamMemberDto(bool isChunk = false) =>
        new()
        {
            Name = "Lucas Vicente",
            Role = "Developer",
            IsChunk = isChunk,
            File = new Mock<IFormFile>().Object,
            FileName = "foto.jpg",
            ChunkIndex = 0,
            TotalChunks = 1,
        };

    public static EntityFile CreateFakeEntityFile() =>
        new EntityFile
        {
            Id = 10,
            CaminhoRelativo = "uploads/foto.jpg",
            NomeArquivo = "foto.jpg",
            FeatureCategoria = "About",
            TamanhoBytes = 12345,
            ContentType = "image/jpeg",
        };
}
