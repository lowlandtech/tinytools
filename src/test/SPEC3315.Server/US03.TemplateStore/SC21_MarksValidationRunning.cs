#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "21")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC21_MarksValidationRunning
{
    private TemplateState _state = new();

    [Given("an editor with a stale validation error")]
    private void Given() => _state = new TemplateState { ValidationError = "old" };

    [When("validation starts")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateValidateStarted());

    public SC21_MarksValidationRunning()
    {
        Given();
        When();
    }

    [Then("the editor should be validating")]
    [Trait("UAC", "19"), Fact]
    public void UAC19_StartSetsValidating() => new { _state.IsValidating, _state.ValidationResult, _state.ValidationError }.Should().BeEquivalentTo(new { IsValidating = true, ValidationResult = (object?)null, ValidationError = (string?)null });
}
#endif
