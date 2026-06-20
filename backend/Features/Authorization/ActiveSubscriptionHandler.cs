using System.Security.Claims;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.MercadoPago.Chargebacks.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Claims.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Payments.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Domain.Entities;
using MeuCrudCsharp.Features.Shared.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;

namespace MeuCrudCsharp.Features.Authorization;

public class ActiveSubscriptionHandler : AuthorizationHandler<ActiveSubscriptionRequirement>
{
    private readonly IMongoDbContext _dbContext;

    public ActiveSubscriptionHandler(IMongoDbContext dbContext)
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

        var subscriptions = _dbContext.GetCollection<Subscription>("subscriptions");

        var hasActiveSubscription = await subscriptions.Find(s => s.UserId == userId && s.Status == "ativo").AnyAsync();

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

