using System;
using MeuCrudCsharp.Features.Caching.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MeuCrudCsharp.Features.Security;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RateLimitAttribute : ActionFilterAttribute
{
    private readonly int _limit;
    private readonly int _seconds;

    public RateLimitAttribute(int limit, int seconds)
    {
        _limit = limit;
        _seconds = seconds;
    }

    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        var userId = context.HttpContext.User.FindFirst("id")?.Value;

        if (userId == null)
        {
            await next();
            return;
        }

        var cacheService = context.HttpContext.RequestServices.GetService<ICacheService>();
        
        if (cacheService == null)
        {
            await next();
            return;
        }

        var key = $"rate_limit:{userId}:{context.ActionDescriptor.DisplayName}";

        var count = await cacheService.IncrementAsync(key, _seconds);

        if (count > _limit)
        {
            context.Result = new ContentResult
            {
                StatusCode = 429,
                Content =
                    $"Você excedeu o limite de requisições. Tente novamente em alguns segundos.",
            };
            return;
        }

        await next();
    }
}
