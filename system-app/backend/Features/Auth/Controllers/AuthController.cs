using System.IdentityModel.Tokens.Jwt;
using System.Security.Policy;
using MeuCrudCsharp.Features.Auth.Dtos;
using MeuCrudCsharp.Features.Auth.Interfaces;
using MeuCrudCsharp.Features.Base;
using MeuCrudCsharp.Features.Caching.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeuCrudCsharp.Features.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        SignInManager<Users> signInManager,
        IAppAuthService authService,
        IJwtService jwtService,
        ILogger<AuthController> logger,
        IConfiguration configuration,
        ICacheService cacheService
    ) : ApiControllerBase
    {
        [HttpGet("google-login")]
        [AllowAnonymous]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth");

            var properties = signInManager.ConfigureExternalAuthenticationProperties(
                "Google",
                redirectUrl
            );

            return new ChallengeResult("Google", properties);
        }

        [HttpGet("google-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback(string? remoteError = null)
        {
            var frontendUrl = configuration["General:BaseUrl"] ?? "http://localhost:5173";
            var frontendCallbackUrl = $"{frontendUrl}/google-callback";

            if (remoteError != null)
            {
                logger.LogError("Erro do provedor externo: {RemoteError}", remoteError);
                return Redirect($"{frontendUrl}/login?error=ExternalProviderError");
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                logger.LogError("Erro ao carregar informações de login externo.");
                return Redirect($"{frontendUrl}/login?error=NoExternalInfo");
            }

            try
            {
                var user = await authService.SignInWithGoogleAsync(info.Principal, HttpContext);

                var tokenString = await jwtService.GenerateJwtTokenAsync(user);

                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                return Redirect($"{frontendCallbackUrl}#token={tokenString}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro no fluxo de login Google.");
                return Redirect($"{frontendUrl}/login?error=ServerException");
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            try
            {
                var userId = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier
                )?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Token inválido.");

                var userSession = await authService.GetAuthenticatedUserDataAsync(userId);

                return Ok(userSession);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao recuperar dados do usuário.");
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var response = await authService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao processar login.");
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var response = await authService.RegisterAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao processar registro.");
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var token = HttpContext
                    .Request.Headers.Authorization.ToString()
                    .Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                    return BadRequest("Token não encontrado.");

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var expiration = jwtToken.ValidTo;
                var now = DateTime.UtcNow;
                var ttl = expiration - now;

                if (ttl.TotalSeconds > 0)
                {
                    await cacheService.GetOrCreateAsync<string>(
                        $"blacklist:{token}",
                        () => Task.FromResult("revoked"), // Valor dummy
                        ttl // Tempo de vida exato
                    );

                    logger.LogInformation(
                        "Token invalidado e adicionado à blacklist por {TtlTotalMinutes} minutos.",
                        ttl.TotalMinutes
                    );
                }

                await signInManager.SignOutAsync();

                return Ok(new { message = "Logout realizado com sucesso." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Erro ao processar logout.");
            }
        }
    }
}
