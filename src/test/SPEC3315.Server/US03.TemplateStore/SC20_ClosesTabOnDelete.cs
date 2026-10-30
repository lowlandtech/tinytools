#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "20")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC20_ClosesTabOnDelete : IClassFixture<UseCases.TemplateStoreUseCase>
{
    private readonly UseCases.TemplateStoreUseCase useCase;
    private readonly Guid _id = Guid.NewGuid();
    private TemplateState _state = new();

    [Given("an open template tab with unsaved changes")]
    private void Given() => _state = new TemplateState
    {
        OpenTabs = [new TemplateOpenTab($"tpl:{_id}", "X", "template", _id, true)],
        ActiveTabKey = $"tpl:{_id}",
        IsDirty = true,
    };

    [When("the delete completes for that template")]
    private void When()
    {
        _state = TemplateReducer.Reduce(_state, new TemplateDeleteCompleted($"tpl:{_id}"));
        useCase.Cases["Tabs"] = new { Count = _state.OpenTabs.Count, _state.IsDirty };
    }

    public SC20_ClosesTabOnDelete(UseCases.TemplateStoreUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Then("the tab should be closed")]
    [Trait("UAC", "18"), Fact]
    public void UAC18_DeleteClosesTab() => useCase.Cases["Tabs"].Should().BeEquivalentTo(new { Count = 0, IsDirty = false });
}
#endif
