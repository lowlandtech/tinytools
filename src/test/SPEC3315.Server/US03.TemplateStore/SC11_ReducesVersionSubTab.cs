#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "11")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC11_ReducesVersionSubTab
{
    private TemplateState _state = new();

    [Given("a template version tab")]
    private void Given() => _state = new TemplateState();

    [When("the json sub tab is selected")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateVersionSubTabChanged("json"));

    public SC11_ReducesVersionSubTab()
    {
        Given();
        When();
    }

    [Then("the version sub tab should be json")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_SubTabIsRemembered() => _state.VersionSubTab.Should().Be("json");
}
#endif