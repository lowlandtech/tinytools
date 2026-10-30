using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Frontend.Features.Templates;

public static class TemplateReducer
{
    public static TemplateState Reduce(TemplateState state, object action) => action switch
    {
        TemplatesLoaded a => state with
        {
            AllItems = a.AllItems,
            VersionCache = a.VersionCache,
            OpenTabs = RefreshTabLabels(state.OpenTabs, a.AllItems),
        },

        FilterTemplates a => state with { SearchText = a.SearchText },

        TemplateToggleNode a => state with
        {
            ExpandedNodes = ToggleExpanded(state.ExpandedNodes, a.NodeKey),
        },

        TemplateEnsureExpanded a => state with
        {
            ExpandedNodes = AddExpanded(state.ExpandedNodes, a.NodeKey),
        },

        TemplateSetCreateContext a => state with
        {
            CreateContextType = state.CreateContextGroupKey == a.GroupKey ? null : a.ContextType,
            CreateContextParentId = state.CreateContextGroupKey == a.GroupKey ? null : a.ParentId,
            CreateContextGroupKey = state.CreateContextGroupKey == a.GroupKey ? null : a.GroupKey,
        },

        // If dirty and selecting a different node, show dialog
        TemplateSelectNode a when state.IsDirty
            && state.SelectedNodeKey is not null
            && state.SelectedNodeKey != a.NodeKey
            => state with
            {
                PendingNodeKey = a.NodeKey,
                PendingLabel = a.Label,
                PendingNodeType = a.NodeType,
                PendingEntityId = a.EntityId,
                ShowDirtyDialog = true,
            },

        TemplateSetSelectedNode a => state with { SelectedNodeKey = a.NodeKey },

        TemplateSelectNode => state, // effect handles loading, dispatches TemplateNodeSelected

        TemplateNodeSelected a => ApplyNodeSelection(state, a.NodeKey, a.Label, a.NodeType, a.EntityId),

        TemplatePinNodeSelected a => ApplyNodeSelection(state, a.NodeKey, a.Label, a.NodeType, a.EntityId, pin: true),

        TemplateCloseTab a => CloseTabReducer(state, a.TabKey),

        TemplateActivateTab a => state with
        {
            ActiveTabKey = a.TabKey,
            SelectedNodeKey = state.OpenTabs.Any(t => t.Key == a.TabKey) ? a.TabKey : state.SelectedNodeKey,
        },

        TemplatePinTab a => state with
        {
            OpenTabs = state.OpenTabs
                .Select(t => t.Key == a.TabKey ? t with { IsPinned = true } : t)
                .ToList(),
        },

        TemplateUnpinTab a => state with
        {
            OpenTabs = state.OpenTabs
                .Select(t => t.Key == a.TabKey ? t with { IsPinned = false } : t)
                .ToList(),
        },

        TemplateRestoreTabs a => state with
        {
            OpenTabs = a.Tabs,
            ActiveTabKey = a.ActiveTabKey ?? "session",
            SelectedNodeKey = a.SelectedNodeKey,
        },

        // ── Editor ──
        TemplateEditorChanged a => a.Field switch
        {
            "content" => state with { VersionContent = a.Value, IsDirty = !state.SuppressDirty || state.IsDirty },
            "text" => state with { VersionText = a.Value, IsDirty = !state.SuppressDirty || state.IsDirty },
            "json" => state with { VersionJson = a.Value, IsDirty = !state.SuppressDirty || state.IsDirty },
            _ => state
        },

        TemplateVersionSubTabChanged a => state with { VersionSubTab = a.SubTab },

        // ── Versions ──
        TemplateVersionsLoaded a => ApplyVersionsLoaded(state, a.TemplateId, a.Versions),

        TemplatePreviousVersion when state.VersionIndex > 0 =>
            ApplyVersionNav(state, state.VersionIndex - 1),
        TemplatePreviousVersion => state,

        TemplateNextVersion when state.VersionIndex < state.Versions.Count - 1 =>
            ApplyVersionNav(state, state.VersionIndex + 1),
        TemplateNextVersion => state,

        TemplateSetVersionIndex a when a.Index >= 0 && a.Index < state.Versions.Count =>
            ApplyVersionNav(state, a.Index),

        TemplateVersionCreated a => state with
        {
            Versions = new[] { a.Version }.Concat(state.Versions).ToList(),
            VersionIndex = 0,
            VersionContent = a.Version.Content,
            VersionText = a.Version.Text ?? string.Empty,
            VersionJson = a.Version.Json ?? string.Empty,
            ValidationResult = null,
            ValidationError = null,
        },

        // ── Dirty tracking ──
        TemplateMarkDirty a when state.SuppressDirty => state,
        TemplateMarkDirty a => state with { IsDirty = a.IsDirty },

        TemplateDirtyCancel => state with
        {
            PendingNodeKey = null,
            PendingLabel = null,
            PendingNodeType = null,
            PendingEntityId = null,
            ShowDirtyDialog = false,
        },

        TemplateDirtySaveCompleted when state.PendingNodeKey is not null =>
            ApplyNodeSelection(state, state.PendingNodeKey, state.PendingLabel!, state.PendingNodeType!, state.PendingEntityId!.Value),

        TemplateDirtySaveCompleted => state with
        {
            IsDirty = false,
            ShowDirtyDialog = false,
        },

        TemplateDirtyDiscardCompleted a =>
            ApplyNodeSelection(state, a.NodeKey, a.Label, a.NodeType, a.EntityId),

        // ── CRUD ──
        TemplateSaveCompleted => state with
        {
            IsDirty = false,
            ValidationResult = null,
            ValidationError = null,
        },

        TemplateDeleteCompleted a => CloseTabReducer(state with { IsDirty = false }, a.TabKey),

        TemplateItemCreated a => ApplyNodeSelection(
            state with { AllItems = a.Items, VersionCache = a.VersionCache },
            a.NodeKey, a.Label, a.NodeType, a.EntityId, pin: true),

        TemplateQuietRefresh a => state with
        {
            AllItems = a.Items,
            VersionCache = a.VersionCache,
            OpenTabs = RefreshTabLabels(state.OpenTabs, a.Items),
        },

        // ── Validation ──
        TemplateValidateStarted => state with
        {
            IsValidating = true,
            ValidationError = null,
            ValidationResult = null,
        },

        TemplateValidateCompleted a => state with
        {
            IsValidating = false,
            ValidationResult = a.Result,
            ValidationError = a.Error,
        },

        TemplateValidateAllStarted => state with { IsValidatingAll = true },

        TemplateValidateAllCompleted a => state with
        {
            IsValidatingAll = false,
            VersionCache = a.UpdatedCache,
        },

        _ => state
    };

    private static TemplateState ApplyNodeSelection(
        TemplateState state, string nodeKey, string label, string nodeType, Guid entityId,
        bool pin = false)
    {
        var tabs = state.OpenTabs.ToList();

        var existing = tabs.FindIndex(t => t.Key == nodeKey);
        if (existing >= 0)
        {
            if (pin && !tabs[existing].IsPinned)
                tabs[existing] = tabs[existing] with { IsPinned = true };

            return state with
            {
                SelectedNodeKey = nodeKey,
                ActiveTabKey = nodeKey,
                OpenTabs = tabs,
                IsDirty = false,
                ShowDirtyDialog = false,
                PendingNodeKey = null,
                PendingLabel = null,
                PendingNodeType = null,
                PendingEntityId = null,
            };
        }

        var newTab = new TemplateOpenTab(nodeKey, label, nodeType, entityId, IsPinned: pin);

        var previewIdx = tabs.FindIndex(t => !t.IsPinned && t.NodeType is not null);
        if (!pin && previewIdx >= 0)
            tabs[previewIdx] = newTab;
        else
            tabs.Add(newTab);

        return state with
        {
            SelectedNodeKey = nodeKey,
            ActiveTabKey = nodeKey,
            OpenTabs = tabs,
            IsDirty = false,
            ShowDirtyDialog = false,
            PendingNodeKey = null,
            PendingLabel = null,
            PendingNodeType = null,
            PendingEntityId = null,
        };
    }

    private static TemplateState CloseTabReducer(TemplateState state, string tabKey)
    {
        var tabs = state.OpenTabs.Where(t => t.Key != tabKey).ToList();
        var activeKey = state.ActiveTabKey;

        if (activeKey == tabKey)
        {
            var idx = state.OpenTabs.ToList().FindIndex(t => t.Key == tabKey);
            activeKey = tabs.Count > 0
                ? tabs[Math.Min(idx, tabs.Count - 1)].Key
                : "session";
        }

        return state with
        {
            OpenTabs = tabs,
            ActiveTabKey = activeKey,
            SelectedNodeKey = activeKey,
        };
    }

    private static TemplateState ApplyVersionsLoaded(TemplateState state, Guid templateId, IReadOnlyList<TemplateVersion> versions)
    {
        var v = versions.Count > 0 ? versions[0] : null;
        return state with
        {
            ActiveVersionTemplateId = templateId,
            Versions = versions,
            VersionIndex = 0,
            VersionContent = v?.Content ?? string.Empty,
            VersionText = v?.Text ?? string.Empty,
            VersionJson = v?.Json ?? string.Empty,
            ValidationResult = null,
            ValidationError = null,
        };
    }

    private static TemplateState ApplyVersionNav(TemplateState state, int newIndex)
    {
        var v = state.Versions[newIndex];
        return state with
        {
            VersionIndex = newIndex,
            VersionContent = v.Content,
            VersionText = v.Text ?? string.Empty,
            VersionJson = v.Json ?? string.Empty,
            ValidationResult = null,
            ValidationError = null,
        };
    }

    private static IReadOnlyList<TemplateOpenTab> RefreshTabLabels(IReadOnlyList<TemplateOpenTab> tabs, IReadOnlyList<Template> items)
    {
        if (tabs.Count == 0) return tabs;
        var templates = items.ToDictionary(t => t.Id);
        var versions = items.SelectMany(t => t.Versions).ToDictionary(v => v.Id);
        var packages = items.SelectMany(t => t.Packages).ToDictionary(p => p.Id);

        return tabs.Select(tab =>
        {
            var name = tab.NodeType switch
            {
                "template" when templates.TryGetValue(tab.EntityId, out var tmpl) => tmpl.Name,
                "version" when versions.TryGetValue(tab.EntityId, out var ver) => ver.Name,
                "package" when packages.TryGetValue(tab.EntityId, out var pkg) => pkg.Name,
                _ => tab.Label
            };
            return name != tab.Label ? tab with { Label = name } : tab;
        }).ToList();
    }

    private static IReadOnlySet<string> ToggleExpanded(IReadOnlySet<string> expanded, string key)
    {
        var set = new HashSet<string>(expanded);
        if (!set.Add(key))
            set.Remove(key);
        return set;
    }

    private static IReadOnlySet<string> AddExpanded(IReadOnlySet<string> expanded, string key)
    {
        if (expanded.Contains(key)) return expanded;
        var set = new HashSet<string>(expanded) { key };
        return set;
    }
}
