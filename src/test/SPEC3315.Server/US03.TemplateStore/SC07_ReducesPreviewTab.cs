#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "07")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC07_ReducesPreviewTab
{
    private TemplateState _state = new();
    private static readonly Guid Node = Guid.NewGuid();

    [Given("a loaded template library")]
    private void Given() => _state = new TemplateState();

    [When("a template node is selected")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateNodeSelected($"tpl:{Node}", "Test", "template", Node));

    public SC07_ReducesPreviewTab()
    {
        Given();
        When();
    }

    [Then("the open tabs should have count 1")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_SelectionOpensOnePreviewTab() => _state.OpenTabs.Count.Should().Be(1);
}
#endif