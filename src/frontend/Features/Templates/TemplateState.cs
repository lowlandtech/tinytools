using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Frontend.Features.Templates;

public record TemplateOpenTab(string Key, string Label, string NodeType, Guid EntityId, bool IsPinned = false);

public record TemplateState
{
    // Data
    public IReadOnlyList<Template> AllItems { get; init; } = [];
    public IReadOnlyDictionary<Guid, TemplateVersion?> VersionCache { get; init; }
        = new Dictionary<Guid, TemplateVersion?>();
    public string SearchText { get; init; } = string.Empty;

    // Tree
    public IReadOnlySet<string> ExpandedNodes { get; init; } = new HashSet<string>();
    public string? SelectedNodeKey { get; init; }

    // Create context
    public string? CreateContextType { get; init; }
    public Guid? CreateContextParentId { get; init; }
    public string? CreateContextGroupKey { get; init; }

    // Dynamic tabs
    public IReadOnlyList<TemplateOpenTab> OpenTabs { get; init; } = [];
    public string? ActiveTabKey { get; init; } = "session";

    // Editor buffers (for active version tab)
    public string VersionContent { get; init; } = string.Empty;
    public string VersionText { get; init; } = string.Empty;
    public string VersionJson { get; init; } = string.Empty;

    // Version navigation per template
    public Guid? ActiveVersionTemplateId { get; init; }
    public IReadOnlyList<TemplateVersion> Versions { get; init; } = [];
    public int VersionIndex { get; init; }

    // Sub-tab within version/template tab
    public string VersionSubTab { get; init; } = "content";

    // Validation
    public TemplateValidation? ValidationResult { get; init; }
    public bool IsValidating { get; init; }
    public bool IsValidatingAll { get; init; }
    public string? ValidationError { get; init; }

    // Dirty tracking
    public bool IsDirty { get; init; }
    public bool SuppressDirty { get; init; }
    public bool ShowDirtyDialog { get; init; }
    public string? PendingNodeKey { get; init; }
    public string? PendingLabel { get; init; }
    public string? PendingNodeType { get; init; }
    public Guid? PendingEntityId { get; init; }

    // Computed
    public TemplateVersion? CurrentVersion =>
        Versions.Count > 0 && VersionIndex >= 0 && VersionIndex < Versions.Count
            ? Versions[VersionIndex]
            : null;

    // Helpers
    public Template? FindTemplate(Guid id) =>
        AllItems.FirstOrDefault(t => t.Id == id);

    public TemplateVersion? FindVersion(Guid id) =>
        AllItems.SelectMany(t => t.Versions).FirstOrDefault(v => v.Id == id);

    public TemplatePackage? FindPackage(Guid id) =>
        AllItems.SelectMany(t => t.Packages).FirstOrDefault(p => p.Id == id);
}
