#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "25")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC25_CancelsDirtyDialog
{
    private TemplateState _state = new();

    [Given("a dirty dialog waiting for a decision")]
    private void Given() => _state = new TemplateState
    {
        ShowDirtyDialog = true,
        PendingNodeKey = "tpl:abc",
    };

    [When("the dialog is cancelled")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateDirtyCancel());

    public SC25_CancelsDirtyDialog()
    {
        Given();
        When();
    }

    [Then("the cancelled dialog should drop its pending node")]
    [Trait("UAC", "23"), Fact]
    public void UAC23_CancelHidesDialog() =>
        new { _state.ShowDirtyDialog, _state.PendingNodeKey }
            .Should().BeEquivalentTo(new { ShowDirtyDialog = false, PendingNodeKey = (string?)null });
}
#endif
