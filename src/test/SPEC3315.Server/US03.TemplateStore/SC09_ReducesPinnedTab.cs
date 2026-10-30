#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "09")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC09_ReducesPinnedTab
{
    private TemplateState _state = new();
    private static readonly Guid Node = Guid.NewGuid();

    [Given("a loaded template library")]
    private void Given() => _state = new TemplateState();

    [When("a template node is pinned")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplatePinNodeSelected($"tpl:{Node}", "A", "template", Node));

    public SC09_ReducesPinnedTab()
    {
        Given();
        When();
    }

    [Then("the opened tab should be pinned")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_PinOpensATab() => new { Count = _state.OpenTabs.Count, Pinned = _state.OpenTabs[0].IsPinned }.Should().BeEquivalentTo(new { Count = 1, Pinned = true });
}
#endif
