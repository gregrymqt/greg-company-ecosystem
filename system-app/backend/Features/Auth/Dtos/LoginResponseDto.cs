namespace MeuCrudCsharp.Features.Auth.Dtos;

/// <summary>
/// DTO retornado após Login/Registro bem-sucedido
/// Contém dados do usuário + tokens de autenticação
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// Dados básicos do usuário autenticado
    /// </summary>
    public UserSessionDto User { get; set; } = new();

    /// <summary>
    /// Token JWT de acesso (válido por 8 horas)
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token de renovação (para implementação futura de refresh)
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Data/hora de expiração do token (UTC)
    /// </summary>
    public DateTime Expiration { get; set; }
}
