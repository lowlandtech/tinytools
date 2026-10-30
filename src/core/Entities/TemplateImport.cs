namespace LowlandTech.TinyTools.Core.Entities;

public class TemplateImport
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid PackageId { get; set; }

    public TemplatePackage Package { get; set; } = null!;
}
