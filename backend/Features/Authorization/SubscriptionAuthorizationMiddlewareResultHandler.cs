using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace MeuCrudCsharp.Features.Authorization;

public class SubscriptionAuthorizationMiddlewareResultHandler
    : IAuthorizationMiddlewareResultHandler
{
    public readonly IAuthorizationMiddlewareResultHandler _defaultHandler =
        new AuthorizationMiddlewareResultHandler();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult
    )
    {
        if (authorizeResult.Forbidden)
        {
            if (
                authorizeResult.AuthorizationFailure!.FailedRequirements.Any(req =>
                    req is ActiveSubscriptionRequirement
                )
            )
            {
                var isApiCall = context
                    .Request.Headers.Accept.ToString()
                    .Contains("application/json");

                if (isApiCall)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(
                        new
                        {
                            error = "active_subscription_required",
                            message = "Uma assinatura ativa é necessária para acessar este recurso.",
                            redirectUrl = "/Payment/CreditCard",
                        }
                    );
                }
                else
                {
                    context.Response.Redirect("/Payment/CreditCard");
                }

                return;
            }
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
