namespace LowlandTech.TinyTools.Rendering;

/// <summary>
/// Re-renders when one of an explicitly enumerated set of input files changes.
///
/// Only the files the caller lists are relevant: a change anywhere else in the tree causes no
/// render, which is what makes `watch` predictable in a shared working directory. The loop
/// polls file timestamps, so it needs no filesystem watcher and stops cleanly when the token
/// is cancelled - no background thread, handle or child process survives the call.
/// </summary>
public sealed class TemplateWatcher(
    IEnumerable<string> relevantFiles,
    Action<IReadOnlyList<string>> onChanged,
    TimeSpan? interval = null)
{
    private readonly string[] _relevant = relevantFiles
        .Select(Path.GetFullPath)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    private readonly Dictionary<string, DateTime> _stamps = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The stamp of a file that has changed but has not settled yet, so one edit is reported once.
    /// </summary>
    private readonly Dictionary<string, DateTime> _unsettled = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The files that can trigger a render, resolved to full paths.</summary>
    public IReadOnlyList<string> RelevantFiles => _relevant;

    /// <summary>How many times a change was observed and reported.</summary>
    public int Revisions { get; private set; }

    /// <summary>Polls until <paramref name="cancellationToken"/> is cancelled.</summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        foreach (var file in _relevant)
        {
            _stamps[file] = Stamp(file);
            _unsettled.Remove(file);
        }

        var delay = interval ?? TimeSpan.FromMilliseconds(250);
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

            var changed = new List<string>();
            foreach (var file in _relevant)
            {
                var stamp = Stamp(file);
                if (stamp == _stamps[file])
                {
                    _unsettled.Remove(file);
                    continue;
                }

                // Writing a file changes it more than once - it is truncated, then filled - so a
                // single edit can be observed as two or more stamps. A change is therefore reported
                // only once its stamp has been seen twice in a row, which makes one edit render once
                // and also stops a half-written file from being rendered.
                if (_unsettled.TryGetValue(file, out var seen) && seen == stamp)
                {
                    _stamps[file] = stamp;
                    _unsettled.Remove(file);
                    changed.Add(file);
                }
                else
                {
                    _unsettled[file] = stamp;
                }
            }

            if (changed.Count == 0) continue;

            Revisions++;
            onChanged(changed);
        }
    }

    private static DateTime Stamp(string file) =>
        File.Exists(file) ? File.GetLastWriteTimeUtc(file) : DateTime.MinValue;
}
