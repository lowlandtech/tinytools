#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "10")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC10_KeepsPinnedTab
{
    private TemplateState _state = new();
    private static readonly Guid Pinned = Guid.NewGuid();
    private static readonly Guid Preview = Guid.NewGuid();

    [Given("a pinned template tab")]
    private void Given() => _state = TemplateReducer.Reduce(new TemplateState(), new TemplatePinNodeSelected($"tpl:{Pinned}", "A", "template", Pinned));

    [When("another template node is selected")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateNodeSelected($"tpl:{Preview}", "B", "template", Preview));

    public SC10_KeepsPinnedTab()
    {
        Given();
        When();
    }

    [Then("the open tabs should have count 2")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_PinnedTabIsNotReplaced() => _state.OpenTabs.Count.Should().Be(2);
}
#endif