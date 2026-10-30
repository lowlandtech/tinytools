namespace LowlandTech.TinyTools.Core.Entities;

public class TemplateFlow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<TemplateStep> Steps { get; set; } = [];
}
