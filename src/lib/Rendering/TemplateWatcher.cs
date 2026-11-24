namespace LowlandTech.TinyTools.Rendering;

/// <summary>
/// Re-renders when one of an explicitly enumerated set of input files changes.
///
/// Only the files the caller lists are relevant: a change anywhere else in the tree causes no
/// render, which is what makes `watch` predictable in a shared working directory. The loop
/// polls file timestamps, so it needs no filesystem watcher and stops cleanly when the token
/// is cancelled - no background thread, handle or child process survives the call.
///
/// An edit is not one write. A file is truncated and then filled, and a large or slow write can
/// pause part-way, so one edit can present several stamps. A change is therefore reported only
/// once its stamp has stayed unchanged for <c>settle</c>, which is what makes one edit render once
/// and keeps a half-written file from being rendered. <c>settle</c> is a duration of its own and
/// not a multiple of <c>interval</c>, because the pause inside a write has nothing to do with how
/// often the loop looks.
/// </summary>
public sealed class TemplateWatcher(
    IEnumerable<string> relevantFiles,
    Action<IReadOnlyList<string>> onChanged,
    TimeSpan? interval = null,
    TimeSpan? settle = null)
{
    private readonly string[] _relevant = relevantFiles
        .Select(Path.GetFullPath)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    private readonly Dictionary<string, DateTime> _stamps = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The stamp a file showed when it last changed, and when that was first seen.</summary>
    private readonly Dictionary<string, (DateTime Stamp, DateTime Since)> _unsettled =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The files that can trigger a render, resolved to full paths.</summary>
    public IReadOnlyList<string> RelevantFiles => _relevant;

    /// <summary>How many times a change was observed and reported.</summary>
    public int Revisions { get; private set; }

    /// <summary>
    /// How long a stamp must stay unchanged before the change it represents is reported. A write
    /// that pauses for longer than this is treated as more than one change, which is the honest
    /// limit of polling timestamps.
    /// </summary>
    public TimeSpan Settle { get; } = settle ?? TimeSpan.FromMilliseconds(250);

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

            var now = DateTime.UtcNow;
            var changed = new List<string>();
            foreach (var file in _relevant)
            {
                var stamp = Stamp(file);
                if (stamp == _stamps[file])
                {
                    _unsettled.Remove(file);
                    continue;
                }

                if (_unsettled.TryGetValue(file, out var pending) && pending.Stamp == stamp)
                {
                    if (now - pending.Since < Settle) { continue; }

                    _stamps[file] = stamp;
                    _unsettled.Remove(file);
                    changed.Add(file);
                }
                else
                {
                    _unsettled[file] = (stamp, now);
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
