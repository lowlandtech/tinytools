#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "12")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC12_ReducesMarkDirty
{
    private TemplateState _state = new();

    [Given("a clean editor")]
    private void Given() => _state = new TemplateState { IsDirty = false };

    [When("the editor is marked dirty")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateMarkDirty());

    public SC12_ReducesMarkDirty()
    {
        Given();
        When();
    }

    [Then("the dirty flag should be true")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_MarkDirtySetsTheFlag() => _state.IsDirty.Should().BeTrue();
}
#endif