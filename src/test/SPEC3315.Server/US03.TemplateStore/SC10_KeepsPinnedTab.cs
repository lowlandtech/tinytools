#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "10")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC10_KeepsPinnedTab : IClassFixture<UseCases.TemplateStoreUseCase>
{
    private readonly UseCases.TemplateStoreUseCase useCase;
    private TemplateState _state = new();
    private static readonly Guid Pinned = Guid.NewGuid();
    private static readonly Guid Preview = Guid.NewGuid();

    [Given("a pinned template tab")]
    private void Given() => _state = TemplateReducer.Reduce(new TemplateState(), new TemplatePinNodeSelected($"tpl:{Pinned}", "A", "template", Pinned));

    [When("another template node is selected")]
    private void When()
    {
        _state = TemplateReducer.Reduce(_state, new TemplateNodeSelected($"tpl:{Preview}", "B", "template", Preview));
        useCase.Cases["Tabs"] = new { Count = _state.OpenTabs.Count, FirstPinned = _state.OpenTabs[0].IsPinned, SecondPinned = _state.OpenTabs[1].IsPinned };
    }

    public SC10_KeepsPinnedTab(UseCases.TemplateStoreUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Then("the pinned tab should be kept")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_PinnedTabIsNotReplaced() => useCase.Cases["Tabs"].Should().BeEquivalentTo(new { Count = 2, FirstPinned = true, SecondPinned = false });
}
#endif
