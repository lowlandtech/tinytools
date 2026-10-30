using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Frontend.Features.Templates;

// ── Data Loading ──
public record TemplatesLoaded(
    List<Template> AllItems,
    Dictionary<Guid, TemplateVersion?> VersionCache);

// ── Filtering ──
public record FilterTemplates(string SearchText);

// ── Tree ──
public record TemplateToggleNode(string NodeKey);
public record TemplateEnsureExpanded(string NodeKey);
public record TemplateSelectNode(string NodeKey, string Label, string NodeType, Guid EntityId);
public record TemplateSetSelectedNode(string NodeKey);
public record TemplateNodeSelected(string NodeKey, string Label, string NodeType, Guid EntityId);
public record TemplatePinNodeSelected(string NodeKey, string Label, string NodeType, Guid EntityId);

// ── Create Context ──
public record TemplateSetCreateContext(string? GroupKey, string? ContextType, Guid? ParentId);

// ── Tabs ──
public record TemplateCloseTab(string TabKey);
public record TemplateActivateTab(string TabKey);
public record TemplatePinTab(string TabKey);
public record TemplateUnpinTab(string TabKey);
public record TemplateRestoreTabs(List<TemplateOpenTab> Tabs, string? ActiveTabKey, string? SelectedNodeKey);

// ── Editor ──
public record TemplateEditorChanged(string Field, string Value);
public record TemplateVersionSubTabChanged(string SubTab);

// ── Versions ──
public record TemplateVersionsLoaded(Guid TemplateId, List<TemplateVersion> Versions);
public record TemplatePreviousVersion;
public record TemplateNextVersion;
public record TemplateSetVersionIndex(int Index);
public record TemplateVersionCreated(TemplateVersion Version);

// ── Dirty Tracking ──
public record TemplateMarkDirty(bool IsDirty = true);
public record TemplateDirtyCancel;
public record TemplateDirtySaveCompleted;
public record TemplateDirtyDiscardCompleted(string NodeKey, string Label, string NodeType, Guid EntityId);

// ── CRUD ──
public record TemplateSaveCompleted;
public record TemplateDeleteCompleted(string TabKey);
public record TemplateItemCreated(List<Template> Items, Dictionary<Guid, TemplateVersion?> VersionCache, string NodeKey, string Label, string NodeType, Guid EntityId);
public record TemplateQuietRefresh(List<Template> Items, Dictionary<Guid, TemplateVersion?> VersionCache);

// ── Validation ──
public record TemplateValidateStarted;
public record TemplateValidateCompleted(TemplateValidation? Result, string? Error);
public record TemplateValidateAllStarted;
public record TemplateValidateAllCompleted(Dictionary<Guid, TemplateVersion?> UpdatedCache);
