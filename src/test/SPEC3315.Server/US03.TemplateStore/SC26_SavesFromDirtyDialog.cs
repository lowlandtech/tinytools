#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "26")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC26_SavesFromDirtyDialog
{
    private TemplateState _state = new();

    [Given("an editor with unsaved changes")]
    private void Given() => _state = new TemplateState { IsDirty = true };

    [When("the pending save completes")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateDirtySaveCompleted());

    public SC26_SavesFromDirtyDialog()
    {
        Given();
        When();
    }

    [Then("the dirty flag should be false")]
    [Trait("UAC", "24"), Fact]
    public void UAC24_SaveClearsDirty() => _state.IsDirty.Should().BeFalse();
}
#endif
