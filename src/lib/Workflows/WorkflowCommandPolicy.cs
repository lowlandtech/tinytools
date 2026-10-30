namespace LowlandTech.TinyTools.Workflows;

/// <summary>
/// The command trust policy for workflow command steps. A workflow is data, so a
/// command step may only start a process whose executable is on this documented list;
/// anything else is refused before a process is created.
///
/// The list is intentionally short: it covers the toolchain commands a template
/// workflow needs (build, source control, GitHub). Editing it is a code change, which
/// is the point - a workflow file cannot widen its own trust.
/// </summary>
public static class WorkflowCommandPolicy
{
    private static readonly HashSet<string> TrustedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "dotnet",
        "git",
        "gh",
    };

    /// <summary>The allowed executable names, in a stable display order.</summary>
    public static IReadOnlyList<string> Allowed => TrustedCommands.OrderBy(name => name, StringComparer.Ordinal).ToList();

    /// <summary>Whether <paramref name="command"/> is a trusted executable name.</summary>
    public static bool IsAllowed(string? command) => IsAllowed(command, WorkflowCommandPolicy.Allowed);

    /// <summary>Whether <paramref name="command"/> is in the supplied trust list.</summary>
    public static bool IsAllowed(string? command, IReadOnlyCollection<string> trustedCommands)
    {
        if (string.IsNullOrWhiteSpace(command)) return false;

        var name = command.Trim();
        return trustedCommands.Any(trusted => string.Equals(trusted, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The refusal message, naming both the refused value and the allowed set.</summary>
    public static string DescribeRefusal(string? command) => DescribeRefusal(command, Allowed);

    /// <summary>The refusal message for the supplied trust list.</summary>
    public static string DescribeRefusal(string? command, IReadOnlyCollection<string> trustedCommands) =>
        $"Command '{command}' is not in the trusted command list ({string.Join(", ", trustedCommands)}). " +
        "Add it to WorkflowCommandPolicy or remove the step.";
}

/// <summary>Raised when a command step names an executable the policy does not trust.</summary>
public sealed class WorkflowCommandRefusedException(string message) : InvalidOperationException(message);
