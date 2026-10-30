#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "22")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC22_StoresValidationResult
{
    private readonly TemplateValidation _result = new(true, "output", "expected");
    private TemplateState _state = new();

    [Given("a validation run in progress")]
    private void Given() => _state = new TemplateState { IsValidating = true };

    [When("validation completes successfully")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateValidateCompleted(_result, null));

    public SC22_StoresValidationResult()
    {
        Given();
        When();
    }

    [Then("the successful validation should be shown without an error")]
    [Trait("UAC", "20"), Fact]
    public void UAC20_CompletedStoresResult() =>
        new { _state.IsValidating, _state.ValidationResult, _state.ValidationError }
            .Should().BeEquivalentTo(new
            {
                IsValidating = false,
                ValidationResult = _result,
                ValidationError = (string?)null,
            });
}
#endif
