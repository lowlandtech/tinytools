using System.Diagnostics;

namespace LowlandTech.TinyTools.Workflows;

/// <summary>
/// Runs a workflow command step: enforces the command trust policy before any process
/// starts, guards the working directory, drains both child pipes concurrently and
/// reports the child exit code with its captured error output.
/// </summary>
public static class WorkflowCommandExecutor
{
    /// <summary>
    /// Runs <paramref name="step"/> under <paramref name="root"/>.
    /// </summary>
    /// <param name="step">The resolved command step.</param>
    /// <param name="root">The workflow root the working directory must stay inside.</param>
    /// <param name="cancellationToken">Cancellation for the child process.</param>
    /// <param name="trustedCommands">
    /// An explicit trust list for this run. When omitted, <see cref="WorkflowCommandPolicy"/>
    /// applies; a caller may widen it deliberately, which is a code decision rather than
    /// something a workflow file can do.
    /// </param>
    public static async Task<WorkflowStepResult> RunAsync(
        WorkflowStep step,
        string root,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? trustedCommands = null)
    {
        if (step is null) throw new ArgumentNullException(nameof(step));

        var command = step.Command?.Trim();
        var trusted = trustedCommands ?? WorkflowCommandPolicy.Allowed;
        if (!WorkflowCommandPolicy.IsAllowed(command, trusted))
        {
            throw new WorkflowCommandRefusedException(WorkflowCommandPolicy.DescribeRefusal(command, trusted));
        }

        var workingDirectory = string.IsNullOrWhiteSpace(step.WorkingDirectory)
            ? root
            : Path.GetFullPath(Path.Combine(root, step.WorkingDirectory));
        if (!IsInside(root, workingDirectory))
        {
            throw new InvalidOperationException($"Working directory escapes workflow root: {step.WorkingDirectory}");
        }

        var startInfo = new ProcessStartInfo(command!)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in step.Args)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start {command}.");

        // Both pipes are drained at the same time: reading one to the end first
        // deadlocks as soon as the child fills the other pipe buffer.
#if NET5_0_OR_GREATER
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
#else
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
#endif
#if NET5_0_OR_GREATER
        await process.WaitForExitAsync(cancellationToken);
#else
        await Task.Run(process.WaitForExit, cancellationToken);
#endif
        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"{command} failed ({process.ExitCode}): {error.Trim()}");
        }

        return new(step.Id, step.Type, true, string.IsNullOrWhiteSpace(output) ? "completed" : output.Trim());
    }

    private static bool IsInside(string root, string path) =>
        string.Equals(path, root, StringComparison.OrdinalIgnoreCase)
        || path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
}
