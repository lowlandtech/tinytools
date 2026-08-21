namespace LowlandTech.TinyTools.Workflows;

/// <summary>Portable workflow contract shared by the CLI and UI hosts.</summary>
public sealed class WorkflowDefinition
{
    public string? Name { get; set; }
    public Dictionary<string, object?> Variables { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<WorkflowStep> Steps { get; set; } = [];
}

public sealed class WorkflowStep
{
    public string? Id { get; set; }
    public string Type { get; set; } = "template";
    public string? Template { get; set; }
    public string? Output { get; set; }
    public string? Command { get; set; }
    public List<string> Args { get; set; } = [];
    public string? WorkingDirectory { get; set; }
    public string When { get; set; } = "always";
    public string? ForEach { get; set; }
    public string Item { get; set; } = "item";
}

public sealed record WorkflowStepResult(string? Id, string Type, bool Changed, string? Message = null);

public sealed record WorkflowResult(IReadOnlyList<WorkflowStepResult> Steps)
{
    public bool Changed => Steps.Any(step => step.Changed);
}
