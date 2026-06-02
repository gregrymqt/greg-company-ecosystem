using MeuCrudCsharp.Features.Auth.Middlewares;

namespace MeuCrudCsharp.Extensions.App;

public static class AuthAppExtensions
{
    public static WebApplication UseAuthFeatures(this WebApplication app)
    {
        app.UseAuthentication();
        
        // Garante que o blacklist intercepte o token logo após a autenticação extrair as claims
        app.UseMiddleware<JwtBlacklistMiddleware>();
        
        app.UseAuthorization();

        return app;
    }
}