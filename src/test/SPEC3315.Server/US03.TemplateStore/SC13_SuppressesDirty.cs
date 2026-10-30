#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "13")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC13_SuppressesDirty
{
    private TemplateState _state = new();

    [Given("an editor that suppresses dirty tracking")]
    private void Given() => _state = new TemplateState { SuppressDirty = true, IsDirty = false };

    [When("the editor is marked dirty")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateMarkDirty());

    public SC13_SuppressesDirty()
    {
        Given();
        When();
    }

    [Then("the dirty flag should be false")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_SuppressedDirtyStaysClean() => _state.IsDirty.Should().BeFalse();
}
#endif