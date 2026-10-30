namespace LowlandTech.TinyTools.Core.Entities;

public class StepIntent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public StepKind Kind { get; set; }
    public StepStatus Status { get; set; }

    public ICollection<TemplateStep> Steps { get; set; } = [];
}
