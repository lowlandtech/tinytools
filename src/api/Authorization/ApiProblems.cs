using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LowlandTech.TinyTools.Api.Authorization;

/// <summary>
/// The failure shapes of the API, as RFC 9457 problem documents. A caller gets the same members
/// from every refusal, so a client parses one error contract instead of one per endpoint. The
/// types are stable identifiers, not links a client is expected to fetch.
/// </summary>
public static class ApiProblems
{
    public const string MediaType = "application/problem+json";
    public const string KeyRequired = "https://lowlandtech.dev/problems/api-key-required";
    public const string KeyRejected = "https://lowlandtech.dev/problems/api-key-rejected";
    public const string MalformedBody = "https://lowlandtech.dev/problems/malformed-body";

    public static ProblemDetails For(HttpContext context, int status, string type, string title, string detail) => new()
    {
        Type = type,
        Title = title,
        Status = status,
        Detail = detail,
        Instance = context.Request.Path,
        Extensions = { ["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier },
    };

    public static Task WriteAsync(HttpContext context, ProblemDetails problem, CancellationToken cancellationToken = default)
    {
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        return context.Response.WriteAsJsonAsync(problem, options: null, contentType: MediaType, cancellationToken);
    }
}
