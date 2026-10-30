#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "11")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC11_ReducesVersionSubTab : IClassFixture<UseCases.TemplateStoreUseCase>
{
    private readonly UseCases.TemplateStoreUseCase useCase;
    private TemplateState _state = new();

    [Given("a template version tab")]
    private void Given() => _state = new TemplateState();

    [When("the json sub tab is selected")]
    private void When()
    {
        _state = TemplateReducer.Reduce(_state, new TemplateVersionSubTabChanged("json"));
        useCase.Cases["VersionSubTab"] = _state.VersionSubTab;
    }

    public SC11_ReducesVersionSubTab(UseCases.TemplateStoreUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Then("the version sub tab should be json")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_SubTabIsRemembered() => ((string)useCase.Cases["VersionSubTab"]!).Should().Be("json");
}
#endif
