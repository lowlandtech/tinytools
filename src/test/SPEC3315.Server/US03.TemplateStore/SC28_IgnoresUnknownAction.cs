#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "28")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC28_IgnoresUnknownAction
{
    private TemplateState _state = new();

    [Given("an editor showing the content sub-tab")]
    private void Given() => _state = new TemplateState { VersionSubTab = "content" };

    [When("an unknown action is dispatched")]
    private void When() => _state = TemplateReducer.Reduce(_state, "unknown");

    public SC28_IgnoresUnknownAction()
    {
        Given();
        When();
    }

    [Then("the version sub-tab should be content")]
    [Trait("UAC", "26"), Fact]
    public void UAC26_UnknownActionIsIgnored() => _state.VersionSubTab.Should().Be("content");
}
#endif
