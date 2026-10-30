#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "05")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC05_ReducesEmptyFilter : IClassFixture<UseCases.TemplateStoreUseCase>
{
    private readonly UseCases.TemplateStoreUseCase useCase;
    private TemplateState _state = new() { SearchText = "Entity" };

    [Given("a filtered library")]
    private void Given() => _state = _state with { SearchText = "Entity" };

    [When("the filter is cleared")]
    private void When()
    {
        _state = TemplateReducer.Reduce(_state, new FilterTemplates(""));
        useCase.Cases["SearchText"] = _state.SearchText;
    }

    public SC05_ReducesEmptyFilter(UseCases.TemplateStoreUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Then("the search text should be empty")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_EmptyFilterClears() => ((string)useCase.Cases["SearchText"]!).Should().BeEmpty();
}
#endif
