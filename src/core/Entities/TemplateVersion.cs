namespace LowlandTech.TinyTools.Core.Entities;

public class TemplateVersion
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string Content { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public string DataType { get; set; } = "Workspace";
    public string? Json { get; set; }
    public string? Text { get; set; }
    public bool? IsValid { get; set; }
    public Guid TemplateId { get; set; }

    public Template Template { get; set; } = null!;
}
