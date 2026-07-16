using MeuCrudCsharp.Features.Profiles.UserAccount.Domain.Interfaces;
using MeuCrudCsharp.Features.Auth.Domain.Interfaces;
using MeuCrudCsharp.Features.Auth.Application.Interfaces;
using MeuCrudCsharp.Features.Files.Application.Interfaces;
using MeuCrudCsharp.Features.Profiles.UserAccount.Application.DTOs;
using MeuCrudCsharp.Features.Profiles.UserAccount.Application.Interfaces;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Features.Shared.Infrastructure.Persistence;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.Application.Services;

public class UserAccountService : IUserAccountService
{
    private readonly IUserAccountRepository _repository;
    private readonly IFileService _fileService;
    private readonly IUserContext _userContext;
    private readonly ILogger<UserAccountService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    private const string CATEGORIA_AVATAR = "UserAvatars";

    public UserAccountService(
        IUserAccountRepository repository,
        IFileService fileService,
        IUserContext userContext,
        ILogger<UserAccountService> logger,
        IUnitOfWork unitOfWork
    )
    {
        _repository = repository;
        _fileService = fileService;
        _userContext = userContext;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<AvatarUpdateResponse> UpdateProfilePictureAsync(IFormFile file)
    {
        var userIdStr = _userContext.GetCurrentUserId().ToString();
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("Usuário não identificado.");

        var user = await _repository.GetUserByIdAsync(userId);
        if (user == null)
            throw new Exception("Usuário não encontrado.");

        string urlFinal;
        string novoIdArquivo;

        if (!string.IsNullOrEmpty(user.AvatarFileId))
        {
            var arquivoSalvo = await _fileService.SubstituirArquivoAsync(
                user.AvatarFileId,
                file
            );
            urlFinal = arquivoSalvo.CaminhoRelativo;
            novoIdArquivo = arquivoSalvo.Id.ToString();
        }
        else
        {
            var arquivoSalvo = await _fileService.SalvarArquivoAsync(file, CATEGORIA_AVATAR);
            urlFinal = arquivoSalvo.CaminhoRelativo;
            novoIdArquivo = arquivoSalvo.Id.ToString();
        }

        user.AvatarFileId = novoIdArquivo;
        user.AvatarUrl = urlFinal;

        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Avatar atualizado para o usuário {UserId}", userId);

        return new AvatarUpdateResponse
        {
            AvatarUrl = urlFinal,
            Message = "Foto de perfil atualizada com sucesso!",
        };
    }

    public async Task<UserProfileDto> GetProfileAsync()
    {
        var userIdStr = _userContext.GetCurrentUserId().ToString();
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("Usuário não identificado.");

        var user = await _repository.GetUserByIdAsync(userId);
        if (user == null)
            throw new Exception("Usuário não encontrado.");

        return new UserProfileDto
        {
            Name = user.Name ?? string.Empty,
            Email = user.Email ?? string.Empty,
            AvatarUrl = user.AvatarUrl ?? string.Empty
        };
    }
}
