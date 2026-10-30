#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "15")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC15_ClampsPreviousVersion
{
    private TemplateState _state = new();

    [Given("a version list already at the first version")]
    private void Given() => _state = new TemplateState
    {
        Versions = [new() { Name = "v1" }, new() { Name = "v2" }],
        VersionIndex = 0,
    };

    [When("the previous version is requested")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplatePreviousVersion());

    public SC15_ClampsPreviousVersion()
    {
        Given();
        When();
    }

    [Then("the version index should be 0")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_PreviousStopsAtTheFirst() => _state.VersionIndex.Should().Be(0);
}
#endif