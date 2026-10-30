#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "16")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC16_ReducesNextVersion : IClassFixture<UseCases.TemplateStoreUseCase>
{
    private readonly UseCases.TemplateStoreUseCase useCase;
    private TemplateState _state = new();

    [Given("a version list positioned on the first version")]
    private void Given() => _state = new TemplateState
    {
        Versions = [new() { Name = "v1", Content = "v1content" }, new() { Name = "v2", Content = "v2" }],
        VersionIndex = 0,
    };

    [When("the next version is requested")]
    private void When()
    {
        _state = TemplateReducer.Reduce(_state, new TemplateNextVersion());
        useCase.Cases["Version"] = new { _state.VersionIndex, _state.VersionContent };
    }

    public SC16_ReducesNextVersion(UseCases.TemplateStoreUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Then("the next version should be shown")]
    [Trait("UAC", "14"), Fact]
    public void UAC14_NextMovesForward() => useCase.Cases["Version"].Should().BeEquivalentTo(new { VersionIndex = 1, VersionContent = "v2" });
}
#endif
