#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "28")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC28_IgnoresUnknownAction
{
    private TemplateState _state = new();
    private TemplateState _before = new();

    [Given("an editor showing the content sub-tab")]
    private void Given()
    {
        _state = new TemplateState { VersionSubTab = "content" };
        _before = _state;
    }

    [When("an unknown action is dispatched")]
    private void When() => _state = TemplateReducer.Reduce(_state, "unknown");

    public SC28_IgnoresUnknownAction()
    {
        Given();
        When();
    }

    [Then("the state should be unchanged")]
    [Trait("UAC", "26"), Fact]
    public void UAC26_UnknownActionIsIgnored() => _state.Should().Be(_before);
}
#endif
