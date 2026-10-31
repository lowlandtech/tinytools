using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace LowlandTech.TinyTools.Api.Authorization;

/// <summary>
/// Answers a refused call with 401 or 403 and a named error. The server does not
/// run an authentication scheme — it gates calls on a shared key — so the default
/// result handler would fail without a forbid scheme instead of refusing the call.
/// </summary>
public sealed class ApiKeyAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (!authorizeResult.Challenged && !authorizeResult.Forbidden)
        {
            await _default.HandleAsync(next, context, policy, authorizeResult);
            return;
        }

        var status = authorizeResult.Challenged
            ? StatusCodes.Status401Unauthorized
            : StatusCodes.Status403Forbidden;

        var problem = status == StatusCodes.Status401Unauthorized
            ? ApiProblems.For(context, status, ApiProblems.KeyRequired, "API key required",
                $"Provide the configured API key in the {ApiKeyPolicy.HeaderName} header.")
            : ApiProblems.For(context, status, ApiProblems.KeyRejected, "API key rejected",
                "The supplied key is not accepted.");

        await ApiProblems.WriteAsync(context, problem);
    }
}
