namespace LowlandTech.TinyTools.Core.Entities;

public class TemplatePackage
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string PackageVersion { get; set; } = string.Empty;

    public ICollection<Template> Templates { get; set; } = [];
    public ICollection<TemplateImport> Imports { get; set; } = [];
}
