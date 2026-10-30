#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "23")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC23_RecordsValidationFailure
{
    private TemplateState _state = new();

    [Given("a validation run in progress")]
    private void Given() => _state = new TemplateState { IsValidating = true };

    [When("validation fails with an error")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateValidateCompleted(null, "Something broke"));

    public SC23_RecordsValidationFailure()
    {
        Given();
        When();
    }

    [Then("the validation error should be Something broke")]
    [Trait("UAC", "21"), Fact]
    public void UAC21_CompletedStoresError() => _state.ValidationError.Should().Be("Something broke");
}
#endif
