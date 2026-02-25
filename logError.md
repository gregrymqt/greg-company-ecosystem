[Fact]
public async Task CreateSectionAsync_WhenChunksAreValid_ShouldAddSectionToRepository()
{
    var aboutchunk = new CreateUpdateAboutSectionDto
    {
        File = new Mock<IFormFile>().Object, // Mock do arquivo
        IsChunk = true,
        ChunkIndex = 0,
        TotalChunks = 1,
        FileName = "test_image.jpg",
        Title = "Test Section",
        Description = "Test Description",
        ImageAlt = "Test Image Alt",
        OrderIndex = 0,
    };

    var result = _aboutService.CreateSectionAsync(aboutchunk);

    Assert.NotNull(result);
    Assert.Equal("Test Section", result.Result.Title);


}
---
public async Task<AboutSectionDto?> CreateSectionAsync(CreateUpdateAboutSectionDto dto)
    {
        var imageUrl = string.Empty;
        int? fileId = null;

        // 1. Lógica de Chunking (Arquivo Grande)
        if (dto is { IsChunk: true, File: not null })
        {
            // Processa o pedaço e vê se completou
            if (dto.FileName != null)
            {
                var tempPath = await _fileService.ProcessChunkAsync(
                    dto.File,
                    dto.FileName,
                    dto.ChunkIndex,
                    dto.TotalChunks
                );

                // Se for null, ainda faltam pedaços. Retorna null para o Controller dar OK.
                if (tempPath == null)
                    return null;

                // Se retornou path, o arquivo está completo na pasta temp! Vamos salvar.
                var arquivoSalvo = await _fileService.SalvarArquivoDoTempAsync(
                    tempPath,
                    dto.FileName,
                    CAT_SECTION
                );
                imageUrl = arquivoSalvo.CaminhoRelativo;
                fileId = arquivoSalvo.Id;
            }
        }
        // 2. Lógica de Upload Normal (Arquivo Pequeno)
        else if (dto.File != null)
        {
            var arquivoSalvo = await _fileService.SalvarArquivoAsync(dto.File, CAT_SECTION);
            imageUrl = arquivoSalvo.CaminhoRelativo;
            fileId = arquivoSalvo.Id;
        }

        // 3. Criação da Entidade (Só roda se não for chunk ou se for o ÚLTIMO chunk)
        var entity = new AboutSection
        {
            Title = dto.Title,
            Description = dto.Description,
            ImageAlt = dto.ImageAlt,
            ImageUrl = imageUrl,
            FileId = fileId,
            OrderIndex = dto.OrderIndex, // Use lógica de Max + 1 se quiser
        };

        await _repository.AddSectionAsync(entity);
        await _unitOfWork.CommitAsync(); // Persiste no banco
        await _cache.RemoveAsync(ABOUT_CACHE_KEY);

        return new AboutSectionDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            ImageUrl = entity.ImageUrl,
            ImageAlt = entity.ImageAlt,
            ContentType = "section1",
        };
    }