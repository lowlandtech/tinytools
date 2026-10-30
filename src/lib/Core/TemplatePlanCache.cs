namespace LowlandTech.TinyTools;

/// <summary>
/// A fixed-capacity, thread-safe least-recently-used cache used for compiled template
/// plans. Reading an entry marks it as most recently used; adding an entry beyond the
/// capacity evicts the least recently used one.
/// </summary>
internal sealed class TemplatePlanCache(int capacity)
{
    private readonly object _gate = new();
    private readonly Dictionary<string, LinkedListNode<(string Source, CompiledTemplate Plan)>> _entries =
        new(StringComparer.Ordinal);
    private readonly LinkedList<(string Source, CompiledTemplate Plan)> _order = new();

    public int Capacity { get; } = capacity;

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _entries.Count;
            }
        }
    }

    /// <summary>Returns the cached plan for the source and marks it most recently used.</summary>
    public bool TryGet(string source, out CompiledTemplate? plan)
    {
        lock (_gate)
        {
            if (!_entries.TryGetValue(source, out var node))
            {
                plan = null;
                return false;
            }

            _order.Remove(node);
            _order.AddFirst(node);
            plan = node.Value.Plan;
            return true;
        }
    }

    /// <summary>Adds a plan as most recently used, evicting the least recently used beyond capacity.</summary>
    public CompiledTemplate Add(string source, Func<CompiledTemplate> create)
    {
        lock (_gate)
        {
            if (_entries.TryGetValue(source, out var existing))
            {
                _order.Remove(existing);
                _order.AddFirst(existing);
                return existing.Value.Plan;
            }

            var plan = create();
            var node = _order.AddFirst((source, plan));
            _entries[source] = node;

            while (_entries.Count > Capacity && _order.Last is { } oldest)
            {
                _order.RemoveLast();
                _entries.Remove(oldest.Value.Source);
            }

            return plan;
        }
    }
}
