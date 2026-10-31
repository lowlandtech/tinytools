using Microsoft.AspNetCore.Diagnostics;

namespace LowlandTech.TinyTools.Api.Authorization;

/// <summary>
/// Keeps a request the framework could not read a 400. Without it <c>UseExceptionHandler</c>
/// answers a malformed body with 500, which tells a caller the server broke when the caller did.
/// </summary>
public sealed class MalformedRequestExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException malformed) return false;

        context.Response.StatusCode = malformed.StatusCode;
        await context.Response.WriteAsJsonAsync(
            new { error = "The request body could not be read." }, cancellationToken);

        return true;
    }
}
