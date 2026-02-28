public async Task DeleteTeamMemberAsync(int id)
    {
        var entity =
            await _repository.GetTeamMemberByIdAsync(id)
            ?? throw new ResourceNotFoundException($"Membro {id} não encontrado.");

        // DELETE: Limpeza do arquivo
        if (entity.FileId.HasValue)
        {
            await _fileService.DeletarArquivoAsync(entity.FileId.Value);
        }

        await _repository.DeleteTeamMemberAsync(entity);
        await _unitOfWork.CommitAsync(); // Persiste no banco
        await _cache.RemoveAsync(ABOUT_CACHE_KEY);
    }