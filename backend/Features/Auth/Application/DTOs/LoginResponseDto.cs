namespace MeuCrudCsharp.Features.Auth.Application.DTOs;

public class LoginResponseDto
{
    public UserSessionDto User { get; set; } = new();
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}
