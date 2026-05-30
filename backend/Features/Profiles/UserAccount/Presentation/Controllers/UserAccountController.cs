using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.Files.Presentation.Attributes;
using MeuCrudCsharp.Features.Profiles.UserAccount.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.Profiles.UserAccount.Presentation.Controllers;

[Route("api/user-account")]
public class UserAccountController : ApiControllerBase
{
    private readonly IUserAccountService _userAccountService;

    public UserAccountController(IUserAccountService userAccountService)
    {
        _userAccountService = userAccountService;
    }

    [HttpPost("avatar")]
    [AllowLargeFile(20)]
    public async Task<IActionResult> UploadProfilePicture([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Nenhum arquivo enviado." });
        }

        try
        {
            var result = await _userAccountService.UpdateProfilePictureAsync(file);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Erro ao atualizar avatar.");
        }
    }
}
