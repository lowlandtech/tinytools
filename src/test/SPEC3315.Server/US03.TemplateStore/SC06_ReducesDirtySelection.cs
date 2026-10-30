#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "06")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC06_ReducesDirtySelection
{
    private TemplateState _state = new();
    private static readonly Guid Current = Guid.NewGuid();
    private static readonly Guid Other = Guid.NewGuid();

    [Given("a dirty editor on one template")]
    private void Given() => _state = new TemplateState
    {
        IsDirty = true,
        SelectedNodeKey = $"tpl:{Current}",
    };

    [When("another template is selected")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateSelectNode($"tpl:{Other}", "B", "template", Other));

    public SC06_ReducesDirtySelection()
    {
        Given();
        When();
    }

    [Then("the dirty dialog should be true")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_DirtySelectionShowsDialog() => _state.ShowDirtyDialog.Should().BeTrue();
}
#endif