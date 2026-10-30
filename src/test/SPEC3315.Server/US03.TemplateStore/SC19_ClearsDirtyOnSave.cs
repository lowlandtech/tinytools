#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "19")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC19_ClearsDirtyOnSave : IClassFixture<UseCases.TemplateStoreUseCase>
{
    private readonly UseCases.TemplateStoreUseCase useCase;
    private TemplateState _state = new();

    [Given("an editor with unsaved changes")]
    private void Given() => _state = new TemplateState { IsDirty = true, ValidationError = "stale" };

    [When("the editor saves")]
    private void When()
    {
        _state = TemplateReducer.Reduce(_state, new TemplateSaveCompleted());
        useCase.Cases["Editor"] = new { _state.IsDirty, _state.ValidationResult, _state.ValidationError };
    }

    public SC19_ClearsDirtyOnSave(UseCases.TemplateStoreUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Then("the editor should be clean")]
    [Trait("UAC", "17"), Fact]
    public void UAC17_SaveClearsDirty() => useCase.Cases["Editor"].Should().BeEquivalentTo(new { IsDirty = false, ValidationResult = (object?)null, ValidationError = (string?)null });
}
#endif
