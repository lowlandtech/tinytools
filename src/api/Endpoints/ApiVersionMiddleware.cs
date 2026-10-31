using LowlandTech.TinyTools.Api.Authorization;

namespace LowlandTech.TinyTools.Api.Endpoints;

/// <summary>
/// The version contract of the template API: version 1 is the default, an explicit
/// <c>api-version: 1</c> is the same contract, and anything else is refused as a caller error with
/// the same problem shape every other refusal uses. The middleware reads one header and sets no
/// other behaviour, so a version cannot silently change how an operation behaves.
/// </summary>
public sealed class ApiVersionMiddleware(RequestDelegate next)
{
    public const string HeaderName = "api-version";
    public const string Current = "1";
    public const string Unsupported = "https://lowlandtech.dev/problems/unsupported-api-version";

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var requested))
        {
            var value = requested.ToString();
            if (!string.Equals(value, Current, StringComparison.Ordinal))
            {
                var problem = ApiProblems.For(context, StatusCodes.Status400BadRequest, Unsupported,
                    "Unsupported API version",
                    $"This host serves version {Current}; the request asked for '{value}'.");
                await ApiProblems.WriteAsync(context, problem);
                return;
            }
        }

        await next(context);
    }
}
