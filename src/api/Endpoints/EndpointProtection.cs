using System.Collections.Concurrent;
using LowlandTech.TinyTools.Api.Authorization;

namespace LowlandTech.TinyTools.Api.Endpoints;

/// <summary>
/// A fixed-window limit per credential. The window is the contract: requests 1 to 60 in one minute
/// are admitted, the next is refused with a retry hint, and the count resets when the window does.
/// The counter lives per credential, so one noisy caller cannot starve another.
/// </summary>
public sealed class RateLimitMiddleware(RequestDelegate next)
{
    public const int Limit = 60;
    public static readonly TimeSpan Window = TimeSpan.FromSeconds(60);
    public const string TooMany = "https://lowlandtech.dev/problems/rate-limited";

    public async Task InvokeAsync(HttpContext context, RateLimitWindow window)
    {
        var key = context.Request.Headers.TryGetValue(ApiKeyPolicy.HeaderName, out var supplied) && supplied.Count > 0
            ? supplied.ToString()
            : "anonymous";

        var now = DateTimeOffset.UtcNow;
        var count = window.Count(key, now);
        if (count > Limit)
        {
            var retry = Math.Max(1, (int)Math.Ceiling((window.Start(key) + Window - now).TotalSeconds));
            context.Response.Headers.RetryAfter = retry.ToString();
            var problem = ApiProblems.For(context, StatusCodes.Status429TooManyRequests, TooMany,
                "Too many requests", $"The limit is {Limit} requests per {Window.TotalSeconds:0} seconds per credential.");
            await ApiProblems.WriteAsync(context, problem);
            return;
        }

        await next(context);
    }
}

/// <summary>The counting state of the limit, kept in one place so a test and the host agree on it.</summary>
public sealed class RateLimitWindow
{
    private readonly ConcurrentDictionary<string, (DateTimeOffset Start, int Count)> _windows = new(StringComparer.Ordinal);

    public int Count(string key, DateTimeOffset now) =>
        _windows.AddOrUpdate(
            key,
            _ => (now, 1),
            (_, current) => now - current.Start >= RateLimitMiddleware.Window ? (now, 1) : (current.Start, current.Count + 1)).Count;

    public DateTimeOffset Start(string key) =>
        _windows.TryGetValue(key, out var current) ? current.Start : DateTimeOffset.UtcNow;
}

/// <summary>
/// The store breaker: three consecutive failures open it, one probe is admitted after the open
/// interval, and a successful probe closes it. While it is open the caller is refused before the
/// handler runs, so a failing store is not asked again until it has had time to recover.
/// </summary>
public sealed class StoreBreaker(IConfiguration configuration)
{
    public const int FailureThreshold = 3;
    public const string Unavailable = "https://lowlandtech.dev/problems/store-unavailable";

    private readonly object _gate = new();
    private int _failures;
    private DateTimeOffset? _openedAt;
    private bool _probeTaken;

    public TimeSpan OpenFor
    {
        get
        {
            var configured = configuration["Tinytools:BreakerOpenSeconds"];
            return int.TryParse(configured, out var seconds) && seconds > 0
                ? TimeSpan.FromSeconds(seconds)
                : TimeSpan.FromSeconds(5);
        }
    }

    public string State
    {
        get
        {
            lock (_gate)
            {
                if (_openedAt is null) return "closed";
                return DateTimeOffset.UtcNow - _openedAt.Value >= OpenFor ? "half-open" : "open";
            }
        }
    }

    /// <summary>Whether the request may proceed: closed always, open never, half-open once.</summary>
    public bool TryEnter()
    {
        lock (_gate)
        {
            if (_openedAt is null) return true;
            if (DateTimeOffset.UtcNow - _openedAt.Value < OpenFor) return false;
            if (_probeTaken) return false;
            _probeTaken = true;
            return true;
        }
    }

    public void RecordFailure()
    {
        lock (_gate)
        {
            if (_openedAt is not null)
            {
                // A failed probe re-opens the window rather than counting toward a new one.
                _openedAt = DateTimeOffset.UtcNow;
                _probeTaken = false;
                return;
            }

            _failures++;
            if (_failures < FailureThreshold) return;
            _openedAt = DateTimeOffset.UtcNow;
            _probeTaken = false;
        }
    }

    public void RecordSuccess()
    {
        lock (_gate)
        {
            _failures = 0;
            _openedAt = null;
            _probeTaken = false;
        }
    }
}

/// <summary>Refuses a request while the store breaker is open, before the handler touches the store.</summary>
public sealed class StoreBreakerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, StoreBreaker breaker)
    {
        if (!breaker.TryEnter())
        {
            var problem = ApiProblems.For(context, StatusCodes.Status503ServiceUnavailable, StoreBreaker.Unavailable,
                "The template store is unavailable",
                "The store failed repeatedly; requests are refused until it recovers.");
            await ApiProblems.WriteAsync(context, problem);
            return;
        }

        await next(context);
    }
}
