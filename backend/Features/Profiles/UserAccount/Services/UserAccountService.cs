using MeuCrudCsharp.Features.Auth.Interfaces;
using MeuCrudCsharp.Features.Files.Interfaces;
using MeuCrudCsharp.Features.Profiles.UserAccount.DTOs;
using MeuCrudCsharp.Features.Profiles.UserAccount.Interfaces;
using MeuCrudCsharp.Features.Shared.Work;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.Services;

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
        var userId = _userContext.GetCurrentUserId().ToString();
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("Usuário não identificado.");

        var user = await _repository.GetUserByIdAsync(userId);
        if (user == null)
            throw new Exception("Usuário não encontrado.");

        string urlFinal;
        int novoIdArquivo;

        if (user.AvatarFileId is > 0)
        {
            var arquivoSalvo = await _fileService.SubstituirArquivoAsync(
                user.AvatarFileId.Value,
                file
            );
            urlFinal = arquivoSalvo.CaminhoRelativo;
            novoIdArquivo = arquivoSalvo.Id;
        }
        else
        {
            var arquivoSalvo = await _fileService.SalvarArquivoAsync(file, CATEGORIA_AVATAR);
            urlFinal = arquivoSalvo.CaminhoRelativo;
            novoIdArquivo = arquivoSalvo.Id;
        }

        user.AvatarFileId = novoIdArquivo;

        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Avatar atualizado para o usuário {UserId}", userId);

        return new AvatarUpdateResponse
        {
            AvatarUrl = urlFinal,
            Message = "Foto de perfil atualizada com sucesso!",
        };
    }
}
