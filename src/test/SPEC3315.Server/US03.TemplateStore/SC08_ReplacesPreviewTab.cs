#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "08")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC08_ReplacesPreviewTab : IClassFixture<UseCases.TemplateStoreUseCase>
{
    private readonly UseCases.TemplateStoreUseCase useCase;
    private TemplateState _state = new();
    private static readonly Guid First = Guid.NewGuid();
    private static readonly Guid Second = Guid.NewGuid();

    [Given("a preview tab for one template")]
    private void Given() => _state = TemplateReducer.Reduce(new TemplateState(), new TemplateNodeSelected($"tpl:{First}", "A", "template", First));

    [When("another template node is selected")]
    private void When()
    {
        _state = TemplateReducer.Reduce(_state, new TemplateNodeSelected($"tpl:{Second}", "B", "template", Second));
        useCase.Cases["Preview"] = new { Count = _state.OpenTabs.Count, Label = _state.OpenTabs[0].Label };
    }

    public SC08_ReplacesPreviewTab(UseCases.TemplateStoreUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Then("the preview tab should be replaced")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_SecondSelectionReplacesPreview() => useCase.Cases["Preview"].Should().BeEquivalentTo(new { Count = 1, Label = "B" });
}
#endif
