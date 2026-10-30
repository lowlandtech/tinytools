#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "14")]
[UserStory("03", "Template reducer produces correct state transitions")]
public sealed class SC14_ReducesPreviousVersion : IClassFixture<UseCases.TemplateStoreUseCase>
{
    private readonly UseCases.TemplateStoreUseCase useCase;
    private TemplateState _state = new();

    [Given("a version list positioned on the second version")]
    private void Given() => _state = new TemplateState
    {
        Versions = [new() { Name = "v1", Content = "v1content" }, new() { Name = "v2", Content = "v2content" }],
        VersionIndex = 1,
    };

    [When("the previous version is requested")]
    private void When()
    {
        _state = TemplateReducer.Reduce(_state, new TemplatePreviousVersion());
        useCase.Cases["Version"] = new { _state.VersionIndex, _state.VersionContent };
    }

    public SC14_ReducesPreviousVersion(UseCases.TemplateStoreUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Then("the previous version should be shown")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_PreviousMovesBack() => useCase.Cases["Version"].Should().BeEquivalentTo(new { VersionIndex = 0, VersionContent = "v1content" });
}
#endif
