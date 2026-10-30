using System.Collections.Concurrent;

namespace RazorLight.Caching;

/// <summary>
/// The cache contract the compatibility engine uses. Keys are compared with
/// <see cref="StringComparer.Ordinal"/>, so casing is significant and every key owns its
/// own entry.
/// </summary>
public interface ICachingProvider
{
    /// <summary>Looks a key up. A missing key returns an explicit miss.</summary>
    TemplateCacheLookupResult RetrieveTemplate(string key);

    /// <summary>
    /// Stores a page factory for <paramref name="key"/>, replacing any previous entry.
    /// The factory is not invoked here.
    /// </summary>
    void CacheTemplate(string key, Func<ITemplatePage> pageFactory);

    /// <summary>Whether the key currently has a cached entry.</summary>
    bool Contains(string key);

    /// <summary>Removes the entry for the key. Removing a missing key is not an error.</summary>
    void Remove(string key);
}

/// <summary>
/// A thread-safe in-memory provider. Lookups are safe while other callers add or remove
/// entries, and every key keeps its own factory.
/// </summary>
public sealed class MemoryCachingProvider : ICachingProvider
{
    private readonly ConcurrentDictionary<string, TemplateCacheItem> _items = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public TemplateCacheLookupResult RetrieveTemplate(string key)
    {
        ValidateKey(key);
        return _items.TryGetValue(key, out var item)
            ? new TemplateCacheLookupResult(item)
            : new TemplateCacheLookupResult();
    }

    /// <inheritdoc />
    public void CacheTemplate(string key, Func<ITemplatePage> pageFactory)
    {
        ValidateKey(key);
        if (pageFactory is null) throw new ArgumentNullException(nameof(pageFactory));
        _items[key] = new TemplateCacheItem(key, pageFactory);
    }

    /// <inheritdoc />
    public bool Contains(string key)
    {
        ValidateKey(key);
        return _items.ContainsKey(key);
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        ValidateKey(key);
        _items.TryRemove(key, out _);
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("A cache key must not be null, empty or whitespace.", nameof(key));
        }
    }
}

/// <summary>A cached entry: its key plus the factory that produces the page.</summary>
public readonly record struct TemplateCacheItem(string Key, Func<ITemplatePage> TemplatePageFactory);

/// <summary>
/// A cache lookup result: an explicit miss, or the entry plus its lazily invoked factory.
/// </summary>
public sealed class TemplateCacheLookupResult
{
    /// <summary>Creates a miss.</summary>
    public TemplateCacheLookupResult()
    {
    }

    internal TemplateCacheLookupResult(TemplateCacheItem template)
    {
        Template = template;
        Success = true;
    }

    /// <summary>The cached entry; default for a miss.</summary>
    public TemplateCacheItem Template { get; }

    /// <summary>Whether the key was cached.</summary>
    public bool Success { get; }
}
