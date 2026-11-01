using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Api.Endpoints;

/// <summary>
/// The write contract of the template API: a create can be retried safely with an idempotency key,
/// an update can be refused when the caller worked from a stale copy, and no template response is
/// cacheable, because the data belongs to one deployment's credential.
/// </summary>
public static class TemplateConditions
{
    public const string IdempotencyHeader = "Idempotency-Key";
    public const string IdempotencyReused = "https://lowlandtech.dev/problems/idempotency-key-reused";
    public const string StaleTemplate = "https://lowlandtech.dev/problems/stale-template";

    /// <summary>How long a key stays replayable. A retry after this window is a new request.</summary>
    public static readonly TimeSpan Retention = TimeSpan.FromHours(24);

    public static string Fingerprint(object payload) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload))))[..32];

    public static void DeclareUncacheable(HttpContext context)
    {
        context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";
    }
}

/// <summary>One replayed create: the payload that produced it and the resource it produced.</summary>
public sealed record IdempotentCreate(string Fingerprint, Guid TemplateId, DateTimeOffset ExpiresAt);

/// <summary>
/// Remembers creates by their idempotency key. Bounded and time-limited, because an unbounded
/// memory of every request is a leak with a friendly name.
/// </summary>
public sealed class IdempotencyStore
{
    public const int Capacity = 512;

    private readonly ConcurrentDictionary<string, IdempotentCreate> _entries = new(StringComparer.Ordinal);

    public bool TryReplay(string key, string fingerprint, out Guid templateId, out bool reused)
    {
        reused = false;
        templateId = Guid.Empty;
        if (!_entries.TryGetValue(key, out var entry)) return false;
        if (entry.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _entries.TryRemove(key, out _);
            return false;
        }

        if (!string.Equals(entry.Fingerprint, fingerprint, StringComparison.Ordinal))
        {
            reused = true;
            return false;
        }

        templateId = entry.TemplateId;
        return true;
    }

    public void Record(string key, string fingerprint, Guid templateId)
    {
        _entries[key] = new IdempotentCreate(fingerprint, templateId, DateTimeOffset.UtcNow + TemplateConditions.Retention);
        if (_entries.Count <= Capacity) return;
        foreach (var stale in _entries.Where(pair => pair.Value.ExpiresAt <= DateTimeOffset.UtcNow).Take(64))
            _entries.TryRemove(stale.Key, out _);
    }
}

/// <summary>The work bounds of the template API, declared once so a request is refused before it runs.</summary>
public static class TemplateBounds
{
    /// <summary>The largest template accepted by render, in UTF-8 bytes.</summary>
    public const int TemplateBytes = 256 * 1024;

    /// <summary>The largest JSON model accepted by render, in UTF-8 bytes.</summary>
    public const int ModelBytes = 1024 * 1024;

    public const string TooLarge = "https://lowlandtech.dev/problems/payload-too-large";

    public static int Bytes(string value) => System.Text.Encoding.UTF8.GetByteCount(value);

    /// <summary>The bound a request exceeds, or null when it fits.</summary>
    public static string? Exceeded(string template, string json) =>
        Bytes(template) > TemplateBytes ? "template"
        : Bytes(json) > ModelBytes ? "json"
        : null;
}