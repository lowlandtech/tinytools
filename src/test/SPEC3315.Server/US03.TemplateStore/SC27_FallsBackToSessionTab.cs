#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "27")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC27_FallsBackToSessionTab : IClassFixture<UseCases.TemplateStoreUseCase>
{
    private readonly UseCases.TemplateStoreUseCase useCase;
    private readonly Guid _id = Guid.NewGuid();
    private TemplateState _state = new();

    [Given("a single open template tab")]
    private void Given() => _state = new TemplateState
    {
        OpenTabs = [new TemplateOpenTab($"tpl:{_id}", "X", "template", _id, true)],
        ActiveTabKey = $"tpl:{_id}",
    };

    [When("that tab is closed")]
    private void When()
    {
        _state = TemplateReducer.Reduce(_state, new TemplateCloseTab($"tpl:{_id}"));
        useCase.Cases["Tabs"] = new { Count = _state.OpenTabs.Count, _state.ActiveTabKey };
    }

    public SC27_FallsBackToSessionTab(UseCases.TemplateStoreUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Then("the session tab should be active")]
    [Trait("UAC", "25"), Fact]
    public void UAC25_CloseFallsBackToSession() => useCase.Cases["Tabs"].Should().BeEquivalentTo(new { Count = 0, ActiveTabKey = "session" });
}
#endif
