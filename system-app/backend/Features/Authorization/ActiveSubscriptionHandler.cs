using System.Security.Claims;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Authorization;

public class ActiveSubscriptionHandler(IDbContextFactory<ApiDbContext> dbContextFactory)
    : AuthorizationHandler<ActiveSubscriptionRequirement>
{

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

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var hasActiveSubscription = await dbContext
            .Set<Subscription>()
            .AnyAsync(s => s.UserId == userId && s.Status == "ativo");

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
