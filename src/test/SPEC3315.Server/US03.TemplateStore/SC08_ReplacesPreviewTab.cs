#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "08")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC08_ReplacesPreviewTab
{
    private TemplateState _state = new();
    private static readonly Guid First = Guid.NewGuid();
    private static readonly Guid Second = Guid.NewGuid();

    [Given("a preview tab for one template")]
    private void Given() => _state = TemplateReducer.Reduce(new TemplateState(), new TemplateNodeSelected($"tpl:{First}", "A", "template", First));

    [When("another template node is selected")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateNodeSelected($"tpl:{Second}", "B", "template", Second));

    public SC08_ReplacesPreviewTab()
    {
        Given();
        When();
    }

    [Then("the open tabs should have count 1")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_SecondSelectionReplacesPreview() => _state.OpenTabs.Count.Should().Be(1);
}
#endif