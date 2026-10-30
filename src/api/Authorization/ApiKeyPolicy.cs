using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace LowlandTech.TinyTools.Api.Authorization;

/// <summary>
/// The single authorization policy of the server. Development accepts anonymous
/// calls so the app and the tests can run locally; any other environment
/// requires the configured API key in <c>X-Api-Key</c>, and refuses every call
/// when no key is configured rather than falling open.
/// </summary>
public static class ApiKeyPolicy
{
    public const string Name = "TinytoolsApiKey";
    public const string HeaderName = "X-Api-Key";
    public const string ConfigurationKey = "Tinytools:ApiKey";

    public static IServiceCollection AddApiKeyPolicy(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, ApiKeyHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, ApiKeyAuthorizationResultHandler>();
        services.AddAuthorization(options =>
            options.AddPolicy(Name, policy => policy
                .AddRequirements(new ApiKeyRequirement())));

        return services;
    }
}

public sealed class ApiKeyRequirement : IAuthorizationRequirement;

public sealed class ApiKeyHandler(IWebHostEnvironment environment, IConfiguration configuration)
    : AuthorizationHandler<ApiKeyRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
    {
        if (environment.IsDevelopment())
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var expected = configuration[ApiKeyPolicy.ConfigurationKey];
        if (string.IsNullOrWhiteSpace(expected)) return Task.CompletedTask;

        if (context.Resource is HttpContext http
            && http.Request.Headers.TryGetValue(ApiKeyPolicy.HeaderName, out var supplied)
            && string.Equals(supplied.ToString(), expected, StringComparison.Ordinal))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
