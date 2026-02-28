using System.Net;
using MeuCrudCsharp.Features.Caching.Interfaces;

namespace MeuCrudCsharp.Features.Auth.Middlewares
{
    public class JwtBlacklistMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtBlacklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ICacheService cacheService)
        {
            var token = context
                .Request.Headers["Authorization"]
                .FirstOrDefault()
                ?.Split(" ")
                .Last();

            if (!string.IsNullOrEmpty(token))
            {
                var isRevoked = await cacheService.GetAsync<string>($"blacklist:{token}");

                if (!string.IsNullOrEmpty(isRevoked))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsJsonAsync(
                        new { message = "Token revogado. Faça login novamente." }
                    );
                    return;
                }
            }

            await _next(context);
        }
    }
}
