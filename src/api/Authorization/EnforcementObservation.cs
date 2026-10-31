using System.Collections.Concurrent;

namespace LowlandTech.TinyTools.Api.Authorization;

/// <summary>
/// What a request would have been refused for, recorded without refusing it. The fields are the
/// bounded, non-secret ones only: no key, no body, no template content and nothing the caller
/// asserted about itself.
/// </summary>
public sealed record EnforcementObservation(
    string TraceId, string Credential, string Peer, string Decision, bool WouldRefuse, DateTimeOffset ObservedAt);

/// <summary>The bound on records kept, so observing traffic cannot become a memory leak.</summary>
public sealed class EnforcementObservationLog
{
    public const int Capacity = 256;

    private readonly ConcurrentQueue<EnforcementObservation> _entries = new();

    public void Record(EnforcementObservation observation)
    {
        _entries.Enqueue(observation);
        while (_entries.Count > Capacity && _entries.TryDequeue(out _)) { }
    }

    public IReadOnlyList<EnforcementObservation> Snapshot() => _entries.ToArray();
}

/// <summary>
/// Carries the request's own observation, and starts it before authorization so the decision the
/// pipeline reaches is recorded on the same object rather than assembled later from globals.
/// </summary>
public sealed class EnforcementObservationContext
{
    public const string ItemKey = "tinytools.enforcement";

    public string TraceId { get; } = Guid.NewGuid().ToString("N");
    public string Credential { get; init; } = "none";
    public string Peer { get; init; } = "unknown";
}

/// <summary>
/// Records what each request was, and what the policy said about it. It never sets a status, adds a
/// delay or changes a result: the handler and the policy remain the only things that decide.
/// </summary>
public sealed class EnforcementObservationMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public Task InvokeAsync(HttpContext context, EnforcementObservationLog log)
    {
        var supplied = context.Request.Headers.ContainsKey(ApiKeyPolicy.HeaderName);
        var observation = new EnforcementObservationContext
        {
            Credential = supplied ? "api-key" : "none",
            Peer = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        };

        context.Items[EnforcementObservationContext.ItemKey] = observation;

        context.Response.OnStarting(() =>
        {
            var status = context.Response.StatusCode;
            var refused = status is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden;
            log.Record(new EnforcementObservation(
                observation.TraceId,
                observation.Credential,
                observation.Peer,
                refused ? "refused" : "accepted",
                WouldRefuse: observation.Credential == "none",
                DateTimeOffset.UtcNow));
            return Task.CompletedTask;
        });

        return next(context);
    }
}
