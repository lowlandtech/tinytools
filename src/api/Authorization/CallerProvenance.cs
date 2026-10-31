using System.Collections.Concurrent;

namespace LowlandTech.TinyTools.Api.Authorization;

/// <summary>What the host knows about a caller, without trusting anything the caller says about itself.</summary>
public sealed record CallerProvenance(string Credential, string Peer, DateTimeOffset ObservedAt);

/// <summary>
/// A bounded, in-memory record of who called and from where. It is deliberately small and holds
/// no request content: it exists so an incident can be attributed and so the request context has a
/// provenance the endpoints can use later.
/// </summary>
public sealed class CallerProvenanceLog
{
    public const int Capacity = 256;

    private readonly ConcurrentQueue<CallerProvenance> _entries = new();

    public void Record(CallerProvenance provenance)
    {
        _entries.Enqueue(provenance);
        while (_entries.Count > Capacity && _entries.TryDequeue(out _))
        {
        }
    }

    public IReadOnlyList<CallerProvenance> Snapshot() => _entries.ToArray();
}

/// <summary>
/// Records every call - accepted or refused - before authorization runs, using only the transport
/// peer and the presence of the configured header. Forwarding and identity headers are client
/// supplied, so they are ignored here; a trusted proxy would have to be configured explicitly.
/// </summary>
public sealed class CallerProvenanceMiddleware(RequestDelegate next)
{
    public const string ItemKey = "tinytools.provenance";

    public async Task InvokeAsync(HttpContext context, CallerProvenanceLog log)
    {
        var supplied = context.Request.Headers.TryGetValue(ApiKeyPolicy.HeaderName, out var values) && values.Count > 0;
        var provenance = new CallerProvenance(
            supplied ? "api-key" : "none",
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            DateTimeOffset.UtcNow);

        log.Record(provenance);
        context.Items[ItemKey] = provenance;

        await next(context);
    }
}
