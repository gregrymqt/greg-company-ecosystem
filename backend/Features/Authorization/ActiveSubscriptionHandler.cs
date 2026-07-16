using System.Security.Claims;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Authorization;

public class ActiveSubscriptionHandler : AuthorizationHandler<ActiveSubscriptionRequirement>
{
    private readonly ApplicationDbContext _dbContext;

    public ActiveSubscriptionHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveSubscriptionRequirement requirement
    )
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        if (!Guid.TryParse(userId, out var userIdGuid))
        {
            context.Fail();
            return;
        }

        var hasActiveSubscription = await _dbContext.Subscriptions.AnyAsync(s =>
            s.UserId == userIdGuid && s.Status == "ativo");

        if (hasActiveSubscription)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
