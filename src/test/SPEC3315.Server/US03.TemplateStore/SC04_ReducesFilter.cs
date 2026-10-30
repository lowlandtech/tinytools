#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "04")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC04_ReducesFilter : IClassFixture<UseCases.TemplateStoreUseCase>
{
    private readonly UseCases.TemplateStoreUseCase useCase;
    private TemplateState _state = new();

    [Given("a loaded template library")]
    private void Given() => _state = new TemplateState { AllItems = [] };

    [When("the filter is applied")]
    private void When()
    {
        _state = TemplateReducer.Reduce(_state, new FilterTemplates("Entity"));
        useCase.Cases["SearchText"] = _state.SearchText;
    }

    public SC04_ReducesFilter(UseCases.TemplateStoreUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Then("the search text should be Entity")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_FilterSetsSearchText() => ((string)useCase.Cases["SearchText"]!).Should().Be("Entity");
}
#endif
