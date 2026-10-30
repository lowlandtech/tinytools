#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "17")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC17_StopsAtTheLastVersion
{
    private TemplateState _state = new();

    [Given("a version list positioned on the last version")]
    private void Given() => _state = new TemplateState
    {
        Versions = [new() { Name = "v1" }, new() { Name = "v2" }],
        VersionIndex = 1,
        VersionContent = "second",
    };

    [When("the next version is requested")]
    private void When() => _state = TemplateReducer.Reduce(_state, new TemplateNextVersion());

    public SC17_StopsAtTheLastVersion()
    {
        Given();
        When();
    }

    [Then("the version index should be 1")]
    [Trait("UAC", "15"), Fact]
    public void UAC15_NextAtEndDoesNotMove() => _state.VersionIndex.Should().Be(1);
}
#endif
