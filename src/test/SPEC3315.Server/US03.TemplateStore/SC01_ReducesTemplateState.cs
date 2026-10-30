#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Template reducer produces correct state transitions")]
public class SC01_Reducer
{
    private static TemplateState Reduce(TemplateState state, object action)
        => TemplateReducer.Reduce(state, action);

    // ── TemplatesLoaded ──

    [Fact(DisplayName = "UAC01: TemplatesLoaded populates AllItems")]
    [Trait(Spec.UAC, "01")]
    public void UAC01_TemplatesLoaded()
    {
        var items = new List<Template>
        {
            new() { Id = Guid.NewGuid(), Name = "Foo" },
            new() { Id = Guid.NewGuid(), Name = "Bar" }
        };
        var cache = new Dictionary<Guid, TemplateVersion?>();

        var state = Reduce(new TemplateState(), new TemplatesLoaded(items, cache));

        state.AllItems.Count.Should().Be(2);
        state.VersionCache.Should().BeSameAs(cache);
    }

    // ── FilterTemplates ──

    [Fact(DisplayName = "UAC02: FilterTemplates sets SearchText")]
    [Trait(Spec.UAC, "02")]
    public void UAC02_Filter()
    {
        var state = new TemplateState
        {
            AllItems = [new() { Name = "EntityClass" }, new() { Name = "DbContextClass" }]
        };

        state = Reduce(state, new FilterTemplates("Entity"));

        state.SearchText.Should().Be("Entity");
    }

    [Fact(DisplayName = "UAC03: Empty filter clears SearchText")]
    [Trait(Spec.UAC, "03")]
    public void UAC03_EmptyFilter()
    {
        var state = new TemplateState
        {
            AllItems = [new() { Name = "A" }, new() { Name = "B" }],
            SearchText = "A",
        };

        state = Reduce(state, new FilterTemplates(""));

        state.SearchText.Should().BeEmpty();
    }

    // ── TemplateNodeSelected ──

    [Fact(DisplayName = "UAC04: NodeSelected when dirty shows dialog")]
    [Trait(Spec.UAC, "04")]
    public void UAC04_SelectDirtyShowsDialog()
    {
        var currentId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var state = new TemplateState
        {
            SelectedNodeKey = $"tpl:{currentId}",
            IsDirty = true,
            OpenTabs = [new TemplateOpenTab($"tpl:{currentId}", "A", "template", currentId, true)]
        };

        state = Reduce(state, new TemplateSelectNode($"tpl:{otherId}", "B", "template", otherId));

        state.ShowDirtyDialog.Should().BeTrue();
        state.PendingNodeKey.Should().Be($"tpl:{otherId}");
        state.SelectedNodeKey.Should().Be($"tpl:{currentId}"); // unchanged
    }

    // ── Preview/Pin Tab Model ──

    [Fact(DisplayName = "UAC05: Single click opens preview tab")]
    [Trait(Spec.UAC, "05")]
    public void UAC05_PreviewTab()
    {
        var id = Guid.NewGuid();
        var state = Reduce(new TemplateState(), new TemplateNodeSelected($"tpl:{id}", "Test", "template", id));

        state.OpenTabs.Count.Should().Be(1);
        state.OpenTabs[0].IsPinned.Should().BeFalse();
        state.ActiveTabKey.Should().Be($"tpl:{id}");
        state.SelectedNodeKey.Should().Be($"tpl:{id}");
    }

    [Fact(DisplayName = "UAC06: Second click replaces preview tab")]
    [Trait(Spec.UAC, "06")]
    public void UAC06_ReplacePreview()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var state = Reduce(new TemplateState(), new TemplateNodeSelected($"tpl:{id1}", "A", "template", id1));
        state = Reduce(state, new TemplateNodeSelected($"tpl:{id2}", "B", "template", id2));

        state.OpenTabs.Count.Should().Be(1);
        state.OpenTabs[0].Label.Should().Be("B");
    }

    [Fact(DisplayName = "UAC07: Double click pins the tab")]
    [Trait(Spec.UAC, "07")]
    public void UAC07_PinTab()
    {
        var id = Guid.NewGuid();
        var state = Reduce(new TemplateState(), new TemplatePinNodeSelected($"tpl:{id}", "A", "template", id));

        state.OpenTabs.Count.Should().Be(1);
        state.OpenTabs[0].IsPinned.Should().BeTrue();
    }

    [Fact(DisplayName = "UAC08: Pinned tab not replaced by new selection")]
    [Trait(Spec.UAC, "08")]
    public void UAC08_PinnedNotReplaced()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var state = Reduce(new TemplateState(), new TemplatePinNodeSelected($"tpl:{id1}", "A", "template", id1));
        state = Reduce(state, new TemplateNodeSelected($"tpl:{id2}", "B", "template", id2));

        state.OpenTabs.Count.Should().Be(2);
        state.OpenTabs[0].IsPinned.Should().BeTrue();
        state.OpenTabs[1].IsPinned.Should().BeFalse();
    }

    // ── Version Sub-Tab ──

    [Fact(DisplayName = "UAC09: VersionSubTabChanged updates sub-tab")]
    [Trait(Spec.UAC, "09")]
    public void UAC09_VersionSubTab()
    {
        var state = Reduce(new TemplateState(), new TemplateVersionSubTabChanged("json"));
        state.VersionSubTab.Should().Be("json");
    }

    // ── TemplateMarkDirty ──

    [Fact(DisplayName = "UAC10: TemplateMarkDirty sets IsDirty")]
    [Trait(Spec.UAC, "10")]
    public void UAC10_MarkDirty()
    {
        var state = Reduce(new TemplateState(), new TemplateMarkDirty());
        state.IsDirty.Should().BeTrue();
    }

    [Fact(DisplayName = "UAC11: MarkDirty is suppressed when SuppressDirty is true")]
    [Trait(Spec.UAC, "11")]
    public void UAC11_SuppressDirty()
    {
        var state = new TemplateState { SuppressDirty = true, IsDirty = false };
        state = Reduce(state, new TemplateMarkDirty());
        state.IsDirty.Should().BeFalse();
    }

    // ── Version Navigation ──

    [Fact(DisplayName = "UAC12: PreviousVersion decrements and loads fields")]
    [Trait(Spec.UAC, "12")]
    public void UAC12_PreviousVersion()
    {
        var v0 = new TemplateVersion { Content = "v1content", Text = "v1text" };
        var v1 = new TemplateVersion { Content = "v2content", Text = "v2text" };
        var state = new TemplateState { Versions = [v0, v1], VersionIndex = 1 };

        state = Reduce(state, new TemplatePreviousVersion());

        state.VersionIndex.Should().Be(0);
        state.VersionContent.Should().Be("v1content");
    }

    [Fact(DisplayName = "UAC13: PreviousVersion at 0 is a no-op")]
    [Trait(Spec.UAC, "13")]
    public void UAC13_PreviousVersionAtZero()
    {
        var state = new TemplateState { Versions = [new()], VersionIndex = 0 };
        var after = Reduce(state, new TemplatePreviousVersion());
        after.VersionIndex.Should().Be(0);
    }

    [Fact(DisplayName = "UAC14: NextVersion increments and loads fields")]
    [Trait(Spec.UAC, "14")]
    public void UAC14_NextVersion()
    {
        var v0 = new TemplateVersion { Content = "v1" };
        var v1 = new TemplateVersion { Content = "v2" };
        var state = new TemplateState { Versions = [v0, v1], VersionIndex = 0 };

        state = Reduce(state, new TemplateNextVersion());

        state.VersionIndex.Should().Be(1);
        state.VersionContent.Should().Be("v2");
    }

    [Fact(DisplayName = "UAC15: NextVersion at end is a no-op")]
    [Trait(Spec.UAC, "15")]
    public void UAC15_NextVersionAtEnd()
    {
        var state = new TemplateState { Versions = [new()], VersionIndex = 0 };
        var after = Reduce(state, new TemplateNextVersion());
        after.VersionIndex.Should().Be(0);
    }

    // ── VersionCreated ──

    [Fact(DisplayName = "UAC16: VersionCreated prepends version and resets index")]
    [Trait(Spec.UAC, "16")]
    public void UAC16_VersionCreated()
    {
        var existing = new TemplateVersion { Content = "old" };
        var created = new TemplateVersion { Content = "new", Text = "txt" };
        var state = new TemplateState { Versions = [existing], VersionIndex = 0 };

        state = Reduce(state, new TemplateVersionCreated(created));

        state.Versions.Count.Should().Be(2);
        state.Versions[0].Should().Be(created);
        state.VersionIndex.Should().Be(0);
        state.VersionContent.Should().Be("new");
        state.VersionText.Should().Be("txt");
    }

    // ── TemplateSaveCompleted ──

    [Fact(DisplayName = "UAC17: SaveCompleted clears dirty and validation")]
    [Trait(Spec.UAC, "17")]
    public void UAC17_SaveCompleted()
    {
        var state = new TemplateState { IsDirty = true, ValidationError = "err" };
        state = Reduce(state, new TemplateSaveCompleted());
        state.IsDirty.Should().BeFalse();
        state.ValidationResult.Should().BeNull();
        state.ValidationError.Should().BeNull();
    }

    // ── TemplateDeleteCompleted ──

    [Fact(DisplayName = "UAC18: DeleteCompleted removes tab")]
    [Trait(Spec.UAC, "18")]
    public void UAC18_DeleteCompleted()
    {
        var id = Guid.NewGuid();
        var tabKey = $"tpl:{id}";
        var state = new TemplateState
        {
            OpenTabs = [new TemplateOpenTab(tabKey, "X", "template", id, true)],
            ActiveTabKey = tabKey,
            IsDirty = true
        };

        state = Reduce(state, new TemplateDeleteCompleted(tabKey));

        state.OpenTabs.Should().BeEmpty();
        state.IsDirty.Should().BeFalse();
    }

    // ── Validation ──

    [Fact(DisplayName = "UAC19: ValidateStarted sets loading state")]
    [Trait(Spec.UAC, "19")]
    public void UAC19_ValidateStarted()
    {
        var state = Reduce(new TemplateState(), new TemplateValidateStarted());
        state.IsValidating.Should().BeTrue();
        state.ValidationResult.Should().BeNull();
        state.ValidationError.Should().BeNull();
    }

    [Fact(DisplayName = "UAC20: ValidateCompleted sets result")]
    [Trait(Spec.UAC, "20")]
    public void UAC20_ValidateCompleted()
    {
        var result = new TemplateValidation(true, "output", "expected");
        var state = Reduce(
            new TemplateState { IsValidating = true },
            new TemplateValidateCompleted(result, null));

        state.IsValidating.Should().BeFalse();
        state.ValidationResult.Should().Be(result);
        state.ValidationError.Should().BeNull();
    }

    [Fact(DisplayName = "UAC21: ValidateCompleted with error sets error")]
    [Trait(Spec.UAC, "21")]
    public void UAC21_ValidateCompletedWithError()
    {
        var state = Reduce(
            new TemplateState { IsValidating = true },
            new TemplateValidateCompleted(null, "Something broke"));

        state.IsValidating.Should().BeFalse();
        state.ValidationResult.Should().BeNull();
        state.ValidationError.Should().Be("Something broke");
    }

    [Fact(DisplayName = "UAC22: ValidateAllStarted and Completed")]
    [Trait(Spec.UAC, "22")]
    public void UAC22_ValidateAll()
    {
        var state = Reduce(new TemplateState(), new TemplateValidateAllStarted());
        state.IsValidatingAll.Should().BeTrue();

        var cache = new Dictionary<Guid, TemplateVersion?>();
        state = Reduce(state, new TemplateValidateAllCompleted(cache));
        state.IsValidatingAll.Should().BeFalse();
        state.VersionCache.Should().BeSameAs(cache);
    }

    // ── Dirty Dialog ──

    [Fact(DisplayName = "UAC23: DirtyCancel clears dialog")]
    [Trait(Spec.UAC, "23")]
    public void UAC23_DirtyCancel()
    {
        var state = new TemplateState
        {
            ShowDirtyDialog = true,
            PendingNodeKey = "tpl:abc"
        };

        state = Reduce(state, new TemplateDirtyCancel());

        state.ShowDirtyDialog.Should().BeFalse();
        state.PendingNodeKey.Should().BeNull();
    }

    [Fact(DisplayName = "UAC24: DirtySaveCompleted clears dirty flag")]
    [Trait(Spec.UAC, "24")]
    public void UAC24_DirtySaveCompleted()
    {
        var state = new TemplateState { IsDirty = true };
        state = Reduce(state, new TemplateDirtySaveCompleted());
        state.IsDirty.Should().BeFalse();
    }

    // ── Close Tab ──

    [Fact(DisplayName = "UAC25: CloseTab removes tab and falls back to session")]
    [Trait(Spec.UAC, "25")]
    public void UAC25_CloseTab()
    {
        var id = Guid.NewGuid();
        var tabKey = $"tpl:{id}";
        var state = new TemplateState
        {
            OpenTabs = [new TemplateOpenTab(tabKey, "X", "template", id, true)],
            ActiveTabKey = tabKey,
        };

        state = Reduce(state, new TemplateCloseTab(tabKey));

        state.OpenTabs.Should().BeEmpty();
        state.ActiveTabKey.Should().Be("session");
    }

    // ── Unknown action ──

    [Fact(DisplayName = "UAC26: Unknown action returns state unchanged")]
    [Trait(Spec.UAC, "26")]
    public void UAC26_UnknownAction()
    {
        var state = new TemplateState { VersionSubTab = "content" };
        var after = Reduce(state, "unknown");
        after.Should().Be(state);
    }
}

#endif
